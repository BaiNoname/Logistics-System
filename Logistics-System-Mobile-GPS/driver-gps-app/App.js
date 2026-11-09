import React, { useState, useEffect, useRef, useCallback } from 'react';
import {
  StyleSheet, Text, View, TextInput, TouchableOpacity,
  Alert, ScrollView, ActivityIndicator, SafeAreaView, Switch, AppState, Image,
} from 'react-native';
import * as ImagePicker from 'expo-image-picker';
import { CameraView, useCameraPermissions, useMicrophonePermissions } from 'expo-camera';
import { activateKeepAwakeAsync, deactivateKeepAwake } from 'expo-keep-awake';
import { File, Directory, Paths } from 'expo-file-system';
import { fetch as expoFetch } from 'expo/fetch';
import { WebView } from 'react-native-webview';
import * as Location from 'expo-location';
import * as TaskManager from 'expo-task-manager';
import AsyncStorage from '@react-native-async-storage/async-storage';

const LOCATION_TASK_NAME = 'background-gps-tracking';
const STORAGE_KEY_SERVER = 'gps_app_server_url';
const STORAGE_KEY_TRIP = 'gps_app_trip_id';
const STORAGE_KEY_VIDEO = 'gps_app_video_enabled';
const STORAGE_KEY_VIDEO_QUEUE = 'gps_app_video_queue';
// Chuyến đã gửi xác nhận, đang chờ Vận hành duyệt (để báo tài xế khi bị TỪ CHỐI / được duyệt).
const STORAGE_KEY_AWAITING = 'gps_app_awaiting_approval';
// Lý do app TỰ TẮT GPS khi đang chạy nền (chuyến bị hủy / đã hoàn tất) — hiện lại khi mở app.
const STORAGE_KEY_STOPPED_REASON = 'gps_app_stopped_reason';
const STATUS_POLL_MS = 120000; // app đang mở: hỏi trạng thái chuyến mỗi 2 phút
const FINISHED_STATUSES = ['Completed', 'PendingConfirmation', 'Cancelled'];

// ---- Camera hành trình (TÙY CHỌN, độc lập với GPS) ----
// Quay từng đoạn 1 phút (giống camera hành trình thật): mỗi đoạn ~8-12 MB ở
// 480p — upload nhẹ, mất mạng chỉ mất 1 đoạn chứ không mất cả chuyến.
const SEGMENT_SECONDS = 60;
const VIDEO_BITRATE = 1_000_000; // ~1 Mbps cho hình 480p (+ tiếng)
const KEEP_AWAKE_TAG = 'dashcam';
const UPLOAD_RETRY_MS = 60000;   // còn đoạn chưa gửi được -> thử lại mỗi 1 phút

// Hàng đợi upload lưu trong AsyncStorage: tắt app/mất mạng vẫn không mất đoạn nào.
async function readQueue() {
  try { return JSON.parse((await AsyncStorage.getItem(STORAGE_KEY_VIDEO_QUEUE)) || '[]'); }
  catch { return []; }
}
async function writeQueue(q) {
  await AsyncStorage.setItem(STORAGE_KEY_VIDEO_QUEUE, JSON.stringify(q));
}

// Khóa hàng đợi: mọi thao tác ĐỌC-SỬA-GHI hàng đợi chạy nối tiếp nhau (thêm đoạn mới trong lúc
// đang gửi không bị ghi đè mất).
let queueLock = Promise.resolve();
function withQueueLock(fn) {
  const run = queueLock.then(fn);
  queueLock = run.catch(() => {});
  return run;
}
const itemKeyOf = (q) => `${q.uri}|${q.startedAt}`;
const tripKeyOf = (q) => `${q.server}|${q.tripId}`;
// Xóa ĐÚNG 1 đoạn khỏi hàng đợi (đọc lại bản mới nhất) — trả về số đoạn còn lại.
function removeFromQueue(item) {
  return withQueueLock(async () => {
    const queue = (await readQueue()).filter(q => itemKeyOf(q) !== itemKeyOf(item));
    await writeQueue(queue);
    return queue.length;
  });
}

// Chuyển file quay xong từ thư mục cache (hệ điều hành có thể tự dọn) sang
// thư mục tài liệu của app, cho tới khi gửi lên server thành công.
// LƯU Ý: dùng moveSync (đồng bộ). Bản trước gọi move() — ở Expo SDK 57 hàm này
// là bất đồng bộ, nên đường dẫn lưu vào hàng đợi vẫn là chỗ CŨ trong khi file đã
// bị chuyển đi -> upload luôn lỗi "Network request failed", kẹt mãi "chờ gửi".
const DASHCAM_DIR_NAME = 'dashcam';
function persistSegmentFile(uri) {
  try {
    const dir = new Directory(Paths.document, DASHCAM_DIR_NAME);
    if (!dir.exists) dir.create({ intermediates: true, idempotent: true });
    const f = new File(uri);
    f.moveSync(dir);
    return f.uri;
  } catch (e) {
    return uri; // không chuyển được thì upload thẳng từ chỗ cũ
  }
}

// Tìm file thật của 1 đoạn trong hàng đợi: đúng đường dẫn đã lưu, hoặc (với các
// đoạn bị lưu sai đường dẫn ở bản cũ) tìm theo TÊN FILE trong thư mục dashcam.
function resolveSegmentFile(uri) {
  try {
    const f = new File(uri);
    if (f.exists) return f.uri;
  } catch (e) {}
  try {
    const name = uri.split('/').pop();
    const moved = new File(new Directory(Paths.document, DASHCAM_DIR_NAME), name);
    if (moved.exists) return moved.uri;
  } catch (e) {}
  return null;
}
function deleteFileQuietly(uri) {
  try { const f = new File(uri); if (f.exists) f.delete(); } catch (e) {}
}

let currentServerUrl = '';
let currentTripId = '';
let lastBgStatusCheck = 0; // chống hỏi trạng thái dồn dập khi ping liên tục bị từ chối

// Ping bị server từ chối -> hỏi trạng thái chuyến (tối đa 1 lần/phút). Chuyến đã HỦY /
// đã hoàn tất / đã báo xong (VD Vận hành hủy đơn, bấm Hoàn tất trên Desktop) -> app TỰ
// TẮT GPS ngay cả khi đang chạy nền, ghi lại lý do để báo tài xế khi mở app. Trước đây
// app bỏ qua phản hồi: tài xế chạy tiếp chuyến đã hủy mà không hề biết.
async function stopIfTripEnded(server, trip) {
  const now = Date.now();
  if (now - lastBgStatusCheck < 60000) return;
  lastBgStatusCheck = now;
  try {
    const resp = await fetch(`${server}/api/trips/${trip}/driver-status`);
    if (!resp.ok) return;
    const st = await resp.json();
    if (!st || !FINISHED_STATUSES.includes(st.status)) return;
    await AsyncStorage.setItem(STORAGE_KEY_STOPPED_REASON, JSON.stringify({
      trip, status: st.status, message: st.message || '',
    }));
    if (await Location.hasStartedLocationUpdatesAsync(LOCATION_TASK_NAME))
      await Location.stopLocationUpdatesAsync(LOCATION_TASK_NAME);
  } catch (e) {}
}

TaskManager.defineTask(LOCATION_TASK_NAME, async ({ data, error }) => {
  if (error) return;
  if (data) {
    const { locations } = data;
    const loc = locations[locations.length - 1];
    if (loc && currentServerUrl && currentTripId) {
      try {
        // Backend .NET thật: POST /api/trips/{id}/ping — tripId nằm TRONG
        // đường dẫn URL (không nằm trong body như bản cũ), field camelCase
        // (lng chứ không phải lon). Nếu chuyến đã báo xong/hoàn tất/hủy, server
        // từ chối ghi (trả lỗi) — không sao, bỏ qua im lặng.
        const resp = await fetch(`${currentServerUrl}/api/trips/${currentTripId}/ping`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            lat: loc.coords.latitude,
            lng: loc.coords.longitude,
            speedKmh: loc.coords.speed != null ? loc.coords.speed * 3.6 : null, // m/s -> km/h
            accuracy: loc.coords.accuracy,
            timestamp: new Date(loc.timestamp).toISOString(),
          }),
        });
        if (!resp.ok) await stopIfTripEnded(currentServerUrl, currentTripId);
      } catch (e) {
        console.error('[Background Task] Submit failed:', e.message);
      }
    }
  }
});

export default function App() {
  const [serverUrl, setServerUrl] = useState('https://trucking.logisticssystem.dpdns.org');
  const [tripId, setTripId] = useState('');
  const [legType, setLegType] = useState(''); // 'Pickup' | 'Delivery' | 'Return' | '' (chưa biết)
  const [tracking, setTracking] = useState(false);
  const [status, setStatus] = useState('Not tracking');
  const [loading, setLoading] = useState(true);
  const [showNav, setShowNav] = useState(false);

  // ---- Kiểm hàng + chụp ảnh hàng trước khi xác nhận chặng ----
  // Tên hàng + số lượng lấy SẴN từ server (tài xế chỉ kiểm đếm rồi tick, không nhập tay).
  const [showConfirm, setShowConfirm] = useState(false);
  const [checklist, setChecklist] = useState(null);      // { lines: [...], photoCount, minPhotos, maxPhotos, legType, orderCode }
  const [checklistError, setChecklistError] = useState('');
  const [checkedIds, setCheckedIds] = useState({});      // { [cargoLineId]: true }
  const [photos, setPhotos] = useState([]);              // [{ uri, status: 'uploading'|'done'|'error', message }]
  const [photoMode, setPhotoMode] = useState(false);     // đang mở camera chụp ảnh -> tạm dừng quay video
  const [sending, setSending] = useState(false);

  // ---- Video hành trình ----
  const [videoEnabled, setVideoEnabled] = useState(false);
  const [recState, setRecState] = useState('off'); // 'off' | 'recording' | 'error'
  const [recError, setRecError] = useState('');
  const [pendingCount, setPendingCount] = useState(0);
  const [uploadedCount, setUploadedCount] = useState(0);
  const [uploadError, setUploadError] = useState(''); // lý do gửi lỗi gần nhất — để biết vì sao "chờ gửi" không giảm
  const [appActive, setAppActive] = useState(AppState.currentState === 'active');
  const [cameraReady, setCameraReady] = useState(false);
  const [retryTick, setRetryTick] = useState(0);
  const [loopActive, setLoopActive] = useState(false); // đang trong vòng quay (kể cả lúc chốt đoạn cuối)
  const [camPermission, requestCamPermission] = useCameraPermissions();
  const [micPermission, requestMicPermission] = useMicrophonePermissions();
  const cameraRef = useRef(null);
  const keepRecordingRef = useRef(false);
  const loopRunningRef = useRef(false);
  const uploadingRef = useRef(false);

  // Lấy loại chặng (Lấy hàng / Giao hàng / Trả hàng) của chuyến này để nút xác
  // nhận hiện ĐÚNG ngữ cảnh. Dùng endpoint /route công khai (không cần đăng nhập)
  // — cùng endpoint trang điều hướng navigate.html đang dùng.
  const fetchLegType = async (server, trip) => {
    if (!server || !trip) { setLegType(''); return; }
    try {
      const resp = await fetch(`${server}/api/trips/${trip}/route`);
      if (!resp.ok) { setLegType(''); return; }
      const info = await resp.json();
      setLegType(info.legType || '');
    } catch (e) {
      setLegType('');
    }
  };

  // Hỏi server chuyến này còn theo dõi được không: đã báo xong / đã hoàn
  // thành / đã hủy -> không cho bật; chặng Giao mà chặng Lấy hàng chưa xong ->
  // phải chờ. Trả về null nếu không kết nối được máy chủ.
  const fetchDriverStatus = async (server, trip) => {
    try {
      const resp = await fetch(`${server}/api/trips/${trip}/driver-status`);
      if (resp.status === 404) {
        // Phân biệt 2 loại 404: server trả JSON {message} = THẬT SỰ không có
        // chuyến này; 404 rỗng = server đang chạy code CŨ, chưa có chức năng kiểm tra.
        let body = null;
        try { body = await resp.json(); } catch (e) {}
        return body && body.message ? { notFound: true } : { serverOutdated: true };
      }
      if (!resp.ok) return null;
      return await resp.json();
    } catch (e) {
      return null;
    }
  };

  const isFinishedStatus = (st) => ['Completed', 'PendingConfirmation', 'Cancelled'].includes(st);

  useEffect(() => {
    (async () => {
      const savedServer = await AsyncStorage.getItem(STORAGE_KEY_SERVER);
      const savedTrip = await AsyncStorage.getItem(STORAGE_KEY_TRIP);
      const savedVideo = await AsyncStorage.getItem(STORAGE_KEY_VIDEO);
      setVideoEnabled(savedVideo === '1');
      setPendingCount((await readQueue()).length);
      if (savedServer) setServerUrl(savedServer);
      if (savedTrip) setTripId(savedTrip);
      currentServerUrl = savedServer || '';
      currentTripId = savedTrip || '';

      try {
        const isRunning = await Location.hasStartedLocationUpdatesAsync(LOCATION_TASK_NAME);
        setTracking(isRunning);
        setStatus(isRunning ? 'Tracking active (running in background)' : 'Not tracking');
        if (isRunning) await fetchLegType(savedServer, savedTrip);
        // Chuyến đã xong / bị hủy / bị từ chối... -> checkTripState (chạy ngay sau khi tải xong)
        // kiểm tra và báo — không kiểm tra 2 nơi (trước đây có thể hiện 2 lần cùng 1 thông báo).
      } catch (e) {}
      setLoading(false);
    })();
  }, []);

  // ================= VIDEO HÀNH TRÌNH =================

  // Gửi lần lượt các đoạn trong hàng đợi. Lỗi mạng / server 5xx -> dừng, thử lại
  // sau. Server trả 4xx (chuyến không tồn tại, file hỏng...) -> bỏ đoạn đó luôn,
  // không thử lại vô hạn.
  const processUploadQueue = useCallback(async () => {
    if (uploadingRef.current) return;
    uploadingRef.current = true;
    try {
      // Đọc LẠI hàng đợi từ bộ nhớ ở MỖI vòng và chỉ xóa đúng đoạn vừa xử lý (qua khóa hàng
      // đợi) — trước đây ghi đè cả bản sao cũ: đoạn mới quay xong trong lúc đang gửi bị MẤT.
      // Đoạn bị server HOÃN (409 — chuyến chưa ghi nhận bắt đầu): hoãn luôn MỌI đoạn cùng chuyến
      // trong lượt này (không gửi lại cả file từng đoạn mỗi phút), gửi tiếp đoạn của chuyến khác.
      const deferredTrips = new Set();
      for (;;) {
        const queue = await readQueue();
        setPendingCount(queue.length);
        const item = queue.find(q => !deferredTrips.has(tripKeyOf(q)));
        if (!item) break;

        const fileUri = resolveSegmentFile(item.uri);
        if (!fileUri) {
          // File không còn trên máy (bị dọn/xóa) -> bỏ đoạn này, không kẹt cả hàng đợi
          setUploadError('Mất file 1 đoạn video trên máy — đã bỏ qua đoạn đó.');
          setPendingCount(await removeFromQueue(item));
          continue;
        }

        // Expo SDK 57: fetch dùng bản của Expo (expo/fetch) — KHÔNG nhận kiểu cũ
        // { uri, name, type } của React Native (báo "Unsupported FormDataPart
        // implementation"), phải đưa thẳng đối tượng File của expo-file-system.
        const form = new FormData();
        form.append('file', new File(fileUri));
        form.append('startedAt', item.startedAt);
        form.append('endedAt', item.endedAt);

        let resp;
        try {
          resp = await expoFetch(`${item.server}/api/trips/${item.tripId}/videos`, { method: 'POST', body: form });
        } catch (e) {
          setUploadError(`Không gửi được (mạng): ${e?.message || e}`);
          break; // mất mạng -> để lần sau
        }

        if (resp.status === 409) {
          let detail = '';
          try { detail = ((await resp.json())?.message || '').slice(0, 150); } catch (e) {}
          setUploadError(`Video chuyến #${item.tripId} tạm hoãn${detail ? ': ' + detail : ''} — sẽ gửi lại sau.`);
          deferredTrips.add(tripKeyOf(item));
          continue;
        }

        // CHỈ bỏ đoạn video khi server từ chối DỨT KHOÁT nội dung đoạn đó
        // (400 dữ liệu sai, 413 file quá lớn, 415 sai định dạng). Các lỗi khác
        // (404/405 do server chưa có chức năng video hoặc domain trỏ nhầm máy,
        // 401/403, 5xx...) đều GIỮ LẠI để gửi lại sau — không được làm mất video.
        const PERMANENT_REJECT = [400, 413, 415];
        if (!resp.ok && !PERMANENT_REJECT.includes(resp.status)) {
          let detail = '';
          try { detail = (await resp.text()).slice(0, 150); } catch (e) {}
          setUploadError(resp.status === 404 || resp.status === 405
            ? `Server chưa có chức năng nhận video (${resp.status}) — kiểm tra domain có trỏ đúng máy chạy API mới không. Video vẫn được giữ lại.`
            : `Server lỗi ${resp.status}${detail ? ': ' + detail : ''} — video vẫn được giữ lại.`);
          break;
        }

        if (resp.ok) {
          setUploadedCount(c => c + 1);
          setUploadError('');
        } else {
          let detail = '';
          try { detail = (await resp.text()).slice(0, 150); } catch (e) {}
          setUploadError(`Server từ chối 1 đoạn (${resp.status})${detail ? ': ' + detail : ''}`);
        }
        deleteFileQuietly(fileUri);
        setPendingCount(await removeFromQueue(item));
      }
    } finally {
      uploadingRef.current = false;
    }
  }, []);

  const enqueueSegment = async (uri, startedAt, endedAt, trip, server) => {
    const savedUri = persistSegmentFile(uri);
    const count = await withQueueLock(async () => {
      const queue = await readQueue();
      queue.push({ uri: savedUri, tripId: trip, server, startedAt: startedAt.toISOString(), endedAt: endedAt.toISOString() });
      await writeQueue(queue);
      return queue.length;
    });
    setPendingCount(count);
    processUploadQueue();
  };

  // Vòng lặp quay: quay 1 đoạn tối đa SEGMENT_SECONDS giây -> đưa vào hàng đợi
  // -> quay đoạn tiếp, cho tới khi tắt video / dừng chuyến / app xuống nền.
  // Giờ bắt đầu-kết thúc lấy theo đồng hồ CỦA CHÍNH điện thoại này — cùng đồng
  // hồ với GPS, nên khớp video với điểm GPS rất chính xác.
  const startRecordingLoop = async () => {
    if (loopRunningRef.current || !cameraRef.current) return;
    loopRunningRef.current = true;
    keepRecordingRef.current = true;
    setLoopActive(true);
    setRecState('recording');
    setRecError('');
    try { await activateKeepAwakeAsync(KEEP_AWAKE_TAG); } catch (e) {}
    try {
      while (keepRecordingRef.current && cameraRef.current) {
        const trip = currentTripId;
        const server = currentServerUrl;
        const startedAt = new Date();
        const result = await cameraRef.current.recordAsync({ maxDuration: SEGMENT_SECONDS });
        const endedAt = new Date();
        if (result?.uri && endedAt - startedAt >= 1000 && trip && server) {
          await enqueueSegment(result.uri, startedAt, endedAt, trip, server);
        }
      }
      setRecState('off');
    } catch (e) {
      setRecState('error');
      setRecError(String(e?.message || e));
      // Camera bận / chưa sẵn sàng -> thử lại sau 3 giây
      setTimeout(() => setRetryTick(t => t + 1), 3000);
    } finally {
      loopRunningRef.current = false;
      setLoopActive(false);
      try { deactivateKeepAwake(KEEP_AWAKE_TAG); } catch (e) {}
    }
  };

  const stopRecordingLoop = () => {
    keepRecordingRef.current = false;
    try { cameraRef.current?.stopRecording(); } catch (e) {}
  };

  // Chỉ quay khi: đã bật video + đang chạy chuyến (GPS đang theo dõi) + app ở
  // màn hình chính (hệ điều hành không cho quay camera khi app chạy nền).
  const shouldRecord = videoEnabled && tracking && appActive && !photoMode;
  useEffect(() => {
    if (shouldRecord && cameraReady) startRecordingLoop();
    else stopRecordingLoop();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [shouldRecord, cameraReady, retryTick]);

  // Giữ camera gắn trên màn hình tới khi đoạn CUỐI được chốt xong (dừng chuyến
  // / tắt video giữa chừng vẫn lưu được đoạn đang quay dở), rồi mới tháo ra.
  const cameraMounted = shouldRecord || loopActive;
  useEffect(() => {
    if (!cameraMounted) setCameraReady(false); // camera tháo ra -> lần sau phải chờ sẵn sàng lại
  }, [cameraMounted]);

  useEffect(() => {
    const sub = AppState.addEventListener('change', s => {
      setAppActive(s === 'active');
      if (s === 'active') processUploadQueue();
    });
    processUploadQueue();
    const timer = setInterval(processUploadQueue, UPLOAD_RETRY_MS);
    return () => { sub.remove(); clearInterval(timer); };
  }, [processUploadQueue]);

  // ================= THEO DÕI TRẠNG THÁI CHUYẾN TỪ SERVER =================
  // App đang mở: hỏi trạng thái mỗi 2 phút (và mỗi lần mở lại app):
  //  - Đang theo dõi mà chuyến đã HỦY / hoàn tất / báo xong -> tự tắt GPS + báo tài xế.
  //  - Đã gửi xác nhận, đang chờ duyệt: bị TỪ CHỐI (chuyến quay lại Đang chạy) -> báo tài
  //    xế bật lại theo dõi và xác nhận lại; được duyệt / bị hủy -> báo kết quả.
  const endedAlertRef = useRef(''); // đã báo "chuyến kết thúc" cho chuyến nào — không báo lặp mỗi 2 phút
  const checkTripState = useCallback(async () => {
    // 1. GPS đã bị TỰ TẮT khi app chạy nền -> báo lý do
    try {
      const raw = await AsyncStorage.getItem(STORAGE_KEY_STOPPED_REASON);
      if (raw) {
        await AsyncStorage.removeItem(STORAGE_KEY_STOPPED_REASON);
        const r = JSON.parse(raw);
        setTracking(false);
        setStatus('Not tracking');
        setShowNav(false);
        Alert.alert(r.status === 'Cancelled' ? '⛔ Chuyến đã bị HỦY' : 'Chuyến đã kết thúc',
          (r.message || `Chuyến #${r.trip} không còn chạy nữa.`) +
          (r.status === 'Cancelled' ? '\n\nĐã tắt theo dõi GPS. Dừng lại và LIÊN HỆ ĐIỀU PHỐI để biết xử lý hàng (nếu đã lấy hàng).' : '\n\nĐã tắt theo dõi GPS.'));
      }
    } catch (e) {}

    const server = currentServerUrl, trip = currentTripId;
    if (!server || !trip) return;

    // 2. Đang theo dõi -> chuyến còn chạy không?
    let running = false;
    try { running = await Location.hasStartedLocationUpdatesAsync(LOCATION_TASK_NAME); } catch (e) {}
    const awaiting = await AsyncStorage.getItem(STORAGE_KEY_AWAITING);
    if (!running && awaiting !== trip) return;

    const st = await fetchDriverStatus(server, trip);
    if (st && st.notFound) { // chuyến không còn trên server -> thôi chờ duyệt
      if (awaiting === trip) await AsyncStorage.removeItem(STORAGE_KEY_AWAITING);
      return;
    }
    if (!st || !st.status) return; // mất mạng / server cũ -> lần sau hỏi lại

    if (running && FINISHED_STATUSES.includes(st.status)) {
      try { await Location.stopLocationUpdatesAsync(LOCATION_TASK_NAME); } catch (e) {}
      setTracking(false);
      setStatus('Not tracking');
      setShowNav(false);
      if (endedAlertRef.current === `${trip}|${st.status}`) return; // đã báo rồi (tắt GPS lỗi -> không báo lặp)
      endedAlertRef.current = `${trip}|${st.status}`;
      Alert.alert(st.status === 'Cancelled' ? '⛔ Chuyến đã bị HỦY' : 'Chuyến đã kết thúc',
        (st.message || 'Chuyến này không còn chạy nữa.') +
        (st.status === 'Cancelled' ? '\n\nĐã tắt theo dõi GPS. Dừng lại và LIÊN HỆ ĐIỀU PHỐI để biết xử lý hàng (nếu đã lấy hàng).' : '\n\nĐã tắt theo dõi GPS.'));
      return;
    }

    if (awaiting === trip && st.status !== 'PendingConfirmation') {
      await AsyncStorage.removeItem(STORAGE_KEY_AWAITING);
      if (st.status === 'InProgress')
        Alert.alert('❌ Vận hành TỪ CHỐI xác nhận',
          `Chuyến #${trip} quay lại trạng thái Đang chạy. Bấm "Bắt đầu theo dõi" để bật lại GPS, rồi kiểm hàng + chụp ảnh và xác nhận LẠI (liên hệ Điều phối nếu cần).`);
      else if (st.status === 'Completed')
        Alert.alert('✅ Đã được duyệt', `Chuyến #${trip} đã được Vận hành duyệt hoàn tất.`);
      else if (st.status === 'Cancelled')
        Alert.alert('⛔ Chuyến đã bị HỦY', st.message || `Chuyến #${trip} đã bị hủy.`);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Chạy SAU khi tải xong cấu hình (trước đó chưa biết server / chuyến -> không hỏi được).
  useEffect(() => {
    if (!appActive || loading) return;
    checkTripState();
    const timer = setInterval(checkTripState, STATUS_POLL_MS);
    // GPS bị TỰ TẮT ở tác vụ nền (chuyến bị hủy / đã xong) trong lúc app đang mở -> phát hiện
    // trong vài giây (đọc cờ lý do, rẻ) thay vì chờ tới lượt hỏi 2 phút.
    const quick = setInterval(async () => {
      try { if (await AsyncStorage.getItem(STORAGE_KEY_STOPPED_REASON)) checkTripState(); } catch (e) {}
    }, 10000);
    return () => { clearInterval(timer); clearInterval(quick); };
  }, [appActive, loading, checkTripState]);

  // Bật/tắt video bất cứ lúc nào (trước hoặc trong chuyến). Tắt video KHÔNG ảnh
  // hưởng GPS — GPS vẫn gửi bình thường.
  const toggleVideo = async (value) => {
    if (value) {
      const cam = camPermission?.granted ? camPermission : await requestCamPermission();
      if (!cam?.granted) {
        Alert.alert('Chưa cấp quyền camera', 'Cần quyền Camera để quay video hành trình.');
        return;
      }
      const mic = micPermission?.granted ? micPermission : await requestMicPermission();
      if (!mic?.granted) {
        Alert.alert('Chưa cấp quyền micro', 'Cần quyền Micro để ghi tiếng trong video hành trình.');
        return;
      }
    }
    setVideoEnabled(value);
    await AsyncStorage.setItem(STORAGE_KEY_VIDEO, value ? '1' : '0');
  };

  const videoStatusText = !videoEnabled
    ? 'Tắt — chuyến này chỉ ghi GPS'
    : !tracking ? 'Sẽ tự quay khi bắt đầu chuyến'
    : !appActive ? 'Tạm dừng (app đang chạy nền)'
    : recState === 'recording' ? '● Đang quay'
    : recState === 'error' ? `Lỗi camera, đang thử lại... ${recError}`
    : 'Đang khởi động camera...';

  const saveConfig = async (url, trip) => {
    await AsyncStorage.setItem(STORAGE_KEY_SERVER, url);
    await AsyncStorage.setItem(STORAGE_KEY_TRIP, trip);
    currentServerUrl = url;
    currentTripId = trip;
  };

  const startTracking = async () => {
    try {
      if (!serverUrl || !tripId) {
        Alert.alert('Missing info', 'Please enter both Server URL and Trip ID.');
        return;
      }

      // Kiểm tra với server TRƯỚC khi bật GPS. Chỉ CHẶN khi server trả lời rõ
      // ràng là không được (đã xong / đã hủy / chờ lấy hàng / không có chuyến).
      // Không kiểm tra được (mất mạng, server chạy bản cũ chưa có chức năng
      // này) thì VẪN cho bật GPS — server tự từ chối điểm GPS nếu chuyến không
      // còn chạy, nên không bao giờ để app "chết" chỉ vì thiếu bước kiểm tra.
      const st = await fetchDriverStatus(serverUrl, tripId);
      if (st && st.notFound) {
        Alert.alert('Không tìm thấy chuyến', `Máy chủ ${serverUrl} không có chuyến #${tripId}. Kiểm tra lại Trip ID, hoặc chuyến được tạo trên database của máy khác.`);
        return;
      }
      if (st && !st.serverOutdated && st.canTrack === false) {
        Alert.alert(
          st.status === 'Cancelled' ? 'Chuyến đã bị hủy'
            : isFinishedStatus(st.status) ? 'Chuyến đã hoàn thành'
            : 'Chưa thể bắt đầu chuyến',
          st.message || 'Chuyến này hiện không thể bắt đầu theo dõi.'
        );
        return;
      }

      const { status: fgStatus } = await Location.requestForegroundPermissionsAsync();
      if (fgStatus !== 'granted') {
        Alert.alert('Permission denied', 'Foreground location permission is required.');
        return;
      }

      const { status: bgStatus } = await Location.requestBackgroundPermissionsAsync();
      if (bgStatus !== 'granted') {
        Alert.alert('Background permission denied', 'Please allow "Always" location access in Settings.');
        return;
      }

      await saveConfig(serverUrl, tripId);
      // Bắt đầu theo dõi lại (VD sau khi bị từ chối) -> bỏ trạng thái "chờ duyệt" / lý do tắt cũ.
      await AsyncStorage.multiRemove([STORAGE_KEY_AWAITING, STORAGE_KEY_STOPPED_REASON]);
      await fetchLegType(serverUrl, tripId);

      await Location.startLocationUpdatesAsync(LOCATION_TASK_NAME, {
        accuracy: Location.Accuracy.High,
        timeInterval: 30000,
        distanceInterval: 0,
        showsBackgroundLocationIndicator: true,
        foregroundService: {
          notificationTitle: 'GPS Tracking Active',
          notificationBody: `Reporting location for Trip #${tripId}`,
          notificationColor: '#2E7D32',
        },
      });

      setTracking(true);
      setStatus('Tracking active (running in background)');
    } catch (e) {
      Alert.alert('Error', String(e.message || e));
    }
  };

  const stopTracking = async () => {
    try {
      const isRunning = await Location.hasStartedLocationUpdatesAsync(LOCATION_TASK_NAME);
      if (isRunning) {
        await Location.stopLocationUpdatesAsync(LOCATION_TASK_NAME);
      }
      setTracking(false);
      setStatus('Not tracking');
      setShowNav(false);
    } catch (e) {
      Alert.alert('Stop Error', String(e.message || e));
    }
  };

  // Nhãn nút xác nhận theo ĐÚNG ngữ cảnh chặng — tài xế biết rõ mình đang xác
  // nhận việc gì (lấy hàng xong khác giao hàng xong về mặt nghiệp vụ).
  const arrivalLabel =
    legType === 'Pickup' ? 'Xác nhận đã LẤY HÀNG xong'
    : legType === 'Delivery' ? 'Xác nhận đã GIAO HÀNG xong'
    : legType === 'Return' ? 'Xác nhận đã TRẢ HÀNG xong'
    : 'Báo đã hoàn thành chuyến';

  // ================= KIỂM HÀNG + ẢNH HÀNG =================
  // Bấm nút xác nhận -> mở màn KIỂM HÀNG: hiện sẵn tên hàng + số lượng của chuyến,
  // tài xế kiểm đếm, tick ĐỦ từng dòng và chụp ít nhất 1 ảnh hàng rồi mới gửi được.
  // Đối tác PHẢI giao đủ — thiếu hàng thì KHÔNG xác nhận, gọi Điều phối.
  const loadChecklist = async () => {
    setChecklistError('');
    try {
      const resp = await fetch(`${serverUrl}/api/trips/${tripId}/cargo-checklist`);
      if (!resp.ok) {
        setChecklistError(resp.status === 404
          ? 'Máy chủ chưa có chức năng kiểm hàng (hoặc không tìm thấy chuyến) — cập nhật server.'
          : `Máy chủ lỗi ${resp.status}.`);
        return;
      }
      const data = await resp.json();
      setChecklist(data);
    } catch (e) {
      setChecklistError('Không kết nối được máy chủ: ' + (e?.message || e));
    }
  };

  const reportArrival = async () => {
    if (!serverUrl || !tripId) return;
    setChecklist(null);
    setCheckedIds({});
    setPhotos([]);
    setShowConfirm(true);
    await loadChecklist();
  };

  const toggleChecked = (id) => setCheckedIds(prev => ({ ...prev, [id]: !prev[id] }));

  const uploadPhoto = async (uri, takenAt) => {
    setPhotos(prev => [...prev, { uri, status: 'uploading', message: '' }]);
    const update = (patch) => setPhotos(prev => prev.map(p => (p.uri === uri ? { ...p, ...patch } : p)));
    try {
      // Giống gửi video: expo/fetch + đối tượng File của expo-file-system.
      const form = new FormData();
      form.append('file', new File(uri));
      form.append('takenAt', takenAt.toISOString());
      const resp = await expoFetch(`${serverUrl}/api/trips/${tripId}/photos`, { method: 'POST', body: form });
      let data = null;
      try { data = await resp.json(); } catch (e) {}
      if (resp.ok && data?.success) update({ status: 'done', message: '' });
      else update({ status: 'error', message: data?.message || `Lỗi ${resp.status}` });
    } catch (e) {
      update({ status: 'error', message: 'Mất mạng — bấm ảnh để gửi lại' });
    }
  };

  const takePhoto = async () => {
    if (photoMode) return; // chống bấm 2 lần mở camera 2 lần
    const perm = await ImagePicker.requestCameraPermissionsAsync();
    if (!perm.granted) {
      Alert.alert('Chưa cấp quyền camera', 'Cần quyền Camera để chụp ảnh hàng hóa.');
      return;
    }
    // Đang quay video hành trình thì tạm dừng để nhả camera cho việc chụp ảnh.
    setPhotoMode(true);
    for (let i = 0; i < 40 && loopRunningRef.current; i++) {
      await new Promise(r => setTimeout(r, 100));
    }
    await new Promise(r => setTimeout(r, 300)); // chờ khung camera hành trình tháo ra hẳn
    let picked = null;
    try {
      const result = await ImagePicker.launchCameraAsync({ quality: 0.5, allowsEditing: false });
      if (!result.canceled && result.assets?.length) picked = result.assets[0].uri;
    } catch (e) {
      Alert.alert('Lỗi camera', String(e?.message || e));
    } finally {
      setPhotoMode(false); // quay video hành trình lại ngay, không đợi gửi ảnh xong
    }
    if (picked) uploadPhoto(picked, new Date());
  };

  const retryPhoto = async (photo) => {
    if (photo.status !== 'error') return;
    setPhotos(prev => prev.filter(p => p.uri !== photo.uri));
    await uploadPhoto(photo.uri, new Date());
  };

  const lines = checklist?.lines || [];
  const allChecked = !!checklist && lines.every(l => checkedIds[l.cargoLineId]);
  const uploadedPhotos = photos.filter(p => p.status === 'done').length + (checklist?.photoCount || 0);
  const minPhotos = checklist?.minPhotos || 1;
  const canSend = allChecked && uploadedPhotos >= minPhotos && !photos.some(p => p.status === 'uploading') && !sending;

  const sendArrival = async () => {
    if (!canSend) return;
    setSending(true);
    try {
      // POST /api/trips/{id}/confirm-arrival — kèm danh sách dòng hàng đã tick đủ.
      // Server kiểm tra lại: đủ mọi dòng + có ảnh hàng, rồi chuyển "Chờ xác nhận".
      const resp = await fetch(`${serverUrl}/api/trips/${tripId}/confirm-arrival`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ note: null, confirmedCargoLineIds: lines.map(l => l.cargoLineId) }),
      });
      const data = await resp.json();
      if (data.success) {
        Alert.alert('Đã gửi', 'Đã gửi xác nhận + ảnh hàng cho bộ phận Vận hành. Chuyến sẽ hoàn tất sau khi được duyệt — app sẽ báo nếu bị từ chối.');
        setShowConfirm(false);
        await AsyncStorage.setItem(STORAGE_KEY_AWAITING, String(tripId));
        await stopTracking();
      } else {
        Alert.alert('Chưa gửi được', data.message || 'Lỗi không xác định');
      }
    } catch (e) {
      Alert.alert('Lỗi mạng', e.message);
    } finally {
      setSending(false);
    }
  };

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <ActivityIndicator size="large" color="#2E7D32" />
      </SafeAreaView>
    );
  }

  // Khung camera nhỏ nổi ở góc — gắn ở NGOÀI 2 màn hình (chính / điều hướng)
  // để chuyển qua lại giữa 2 màn hình KHÔNG làm đứt đoạn video đang quay.
  const cameraOverlay = cameraMounted ? (
    <View style={styles.camBox} pointerEvents="none">
      <CameraView
        ref={cameraRef}
        style={StyleSheet.absoluteFill}
        facing="back"
        mode="video"
        videoQuality="480p"
        videoBitrate={VIDEO_BITRATE}
        mute={false}
        onCameraReady={() => setCameraReady(true)}
        onMountError={(e) => { setRecState('error'); setRecError(e?.message || 'Không mở được camera'); }}
      />
      <View style={styles.recBadge}>
        <Text style={styles.recBadgeText}>{recState === 'recording' ? '● REC' : '…'}</Text>
      </View>
    </View>
  ) : null;

  const navScreen = (
      <SafeAreaView key="nav" style={{ flex: 1 }}>
        <WebView
          source={{ uri: `${serverUrl}/navigate.html?tripId=${tripId}` }}
          style={{ flex: 1 }}
          geolocationEnabled={true}
          startInLoadingState={true}
          renderLoading={() => <ActivityIndicator size="large" color="#1565C0" style={{ flex: 1 }} />}
          onError={(e) => Alert.alert('Navigation Error', JSON.stringify(e.nativeEvent))}
        />
        <TouchableOpacity style={styles.btnCloseNav} onPress={() => setShowNav(false)}>
          <Text style={styles.btnText}>Close Navigation</Text>
        </TouchableOpacity>
      </SafeAreaView>
  );

  const legWord = (checklist?.legType || legType) === 'Pickup' ? 'NHẬN' : 'GIAO';
  const confirmScreen = (
    <SafeAreaView key="confirm" style={styles.container}>
      {/* chừa lề phải cho khung camera hành trình nhỏ ở góc */}
      <ScrollView contentContainerStyle={[styles.scroll, cameraMounted && { paddingRight: 128 }]}>
        <Text style={styles.title}>Kiểm hàng</Text>
        <Text style={styles.subtitle}>
          Chuyến #{tripId}{checklist?.orderCode ? ` · Đơn ${checklist.orderCode}` : ''}
        </Text>

        <View style={styles.warnBox}>
          <Text style={styles.warnText}>
            {(checklist?.legType || legType) === 'Pickup'
              ? 'Kiểm đếm hàng đối tác giao. Đối tác PHẢI giao ĐỦ số lượng dưới đây — nếu thiếu, KHÔNG xác nhận, gọi Điều phối.'
              : 'Kiểm đếm hàng bàn giao. Tick từng dòng khi đã giao ĐỦ, chụp ảnh hàng lúc bàn giao làm bằng chứng.'}
          </Text>
        </View>

        {checklistError !== '' && <Text style={styles.videoError}>⚠️ {checklistError}</Text>}
        {!checklist && checklistError === '' && <ActivityIndicator size="large" color="#2E7D32" style={{ marginTop: 20 }} />}
        {checklistError !== '' && (
          <TouchableOpacity onPress={loadChecklist} style={styles.btnRetry}>
            <Text style={styles.btnRetryText}>🔄 Tải lại</Text>
          </TouchableOpacity>
        )}

        {lines.map(l => (
          <TouchableOpacity key={l.cargoLineId} style={[styles.cargoRow, checkedIds[l.cargoLineId] && styles.cargoRowChecked]}
            onPress={() => toggleChecked(l.cargoLineId)}>
            <Text style={styles.cargoCheck}>{checkedIds[l.cargoLineId] ? '☑' : '☐'}</Text>
            <View style={{ flex: 1 }}>
              <Text style={styles.cargoName}>{l.cargoType}{l.isFragile ? ' · Dễ vỡ' : ''}{l.isHazmat ? ' · Nguy hiểm' : ''}</Text>
              <Text style={styles.cargoQty}>
                Số lượng: {l.qty} · khoảng {l.packagesApprox} kiện · {Math.round(l.weightKg)} kg
              </Text>
              <Text style={styles.cargoHint}>Bấm để xác nhận đã {legWord} ĐỦ</Text>
            </View>
          </TouchableOpacity>
        ))}

        {checklist && (
          <View style={styles.videoCard}>
            <Text style={styles.videoTitle}>📷 Ảnh hàng hóa ({uploadedPhotos}/{checklist.maxPhotos || 10})</Text>
            <Text style={styles.videoStatus}>Chụp ít nhất {minPhotos} ảnh rõ hàng trên xe / lúc bàn giao.</Text>
            <View style={styles.photoRow}>
              {photos.map(p => (
                <TouchableOpacity key={p.uri} onPress={() => retryPhoto(p)} style={styles.photoThumbBox}>
                  <Image source={{ uri: p.uri }} style={styles.photoThumb} />
                  <Text style={[styles.photoState, p.status === 'error' && { color: '#C62828' }]}>
                    {p.status === 'done' ? '✅ Đã gửi' : p.status === 'uploading' ? '⏳ Đang gửi' : '⚠️ Gửi lại'}
                  </Text>
                </TouchableOpacity>
              ))}
            </View>
            {photos.some(p => p.status === 'error') && (
              <Text style={styles.videoError}>{photos.find(p => p.status === 'error')?.message}</Text>
            )}
            <TouchableOpacity style={[styles.btnPrimary, (photoMode || uploadedPhotos >= (checklist.maxPhotos || 10)) && styles.btnDisabled]}
              onPress={takePhoto} disabled={photoMode || uploadedPhotos >= (checklist.maxPhotos || 10)}>
              <Text style={styles.btnText}>📷 Chụp ảnh hàng</Text>
            </TouchableOpacity>
          </View>
        )}

        <TouchableOpacity style={[styles.btnSuccess, !canSend && styles.btnDisabled]} onPress={sendArrival} disabled={!canSend}>
          <Text style={styles.btnText}>{sending ? 'Đang gửi...' : `✅ ${arrivalLabel}`}</Text>
        </TouchableOpacity>
        {!allChecked && lines.length > 0 && <Text style={styles.note}>Tick đủ tất cả dòng hàng để gửi.</Text>}
        {allChecked && uploadedPhotos < minPhotos && <Text style={styles.note}>Chụp ít nhất {minPhotos} ảnh hàng để gửi.</Text>}

        <TouchableOpacity style={styles.btnDanger} onPress={() => setShowConfirm(false)}>
          <Text style={styles.btnText}>Quay lại</Text>
        </TouchableOpacity>
      </ScrollView>
    </SafeAreaView>
  );

  const mainScreen = (
    <SafeAreaView key="main" style={styles.container}>
      <ScrollView contentContainerStyle={styles.scroll}>
        <Text style={styles.title}>Delivery GPS Tracker</Text>
        <Text style={styles.subtitle}>Driver App</Text>

        <View style={[styles.statusBox, tracking ? styles.statusActive : styles.statusInactive]}>
          <Text style={styles.statusText}>{status}</Text>
        </View>

        <Text style={styles.label}>Server URL</Text>
        <TextInput
          style={styles.input}
          placeholder="https://xxxx.ngrok-free.app"
          value={serverUrl}
          onChangeText={setServerUrl}
          autoCapitalize="none"
          editable={!tracking}
        />

        <Text style={styles.label}>Trip ID</Text>
        <TextInput
          style={styles.input}
          placeholder="e.g. 42"
          value={tripId}
          onChangeText={setTripId}
          keyboardType="numeric"
          editable={!tracking}
        />

        <View style={styles.videoCard}>
          <View style={styles.videoRow}>
            <Text style={styles.videoTitle}>🎥 Quay video hành trình</Text>
            <Switch
              value={videoEnabled}
              onValueChange={toggleVideo}
              trackColor={{ true: '#81C784', false: '#ccc' }}
              thumbColor={videoEnabled ? '#2E7D32' : '#f4f3f4'}
            />
          </View>
          <Text style={styles.videoStatus}>{videoStatusText}</Text>
          {(pendingCount > 0 || uploadedCount > 0) && (
            <Text style={styles.videoStatus}>
              Đã gửi {uploadedCount} đoạn{pendingCount > 0 ? ` • chờ gửi ${pendingCount} đoạn` : ''}
            </Text>
          )}
          {pendingCount > 0 && uploadError !== '' && (
            <Text style={styles.videoError}>⚠️ {uploadError}</Text>
          )}
          {pendingCount > 0 && (
            <TouchableOpacity onPress={processUploadQueue} style={styles.btnRetry}>
              <Text style={styles.btnRetryText}>🔄 Gửi lại ngay</Text>
            </TouchableOpacity>
          )}
          {videoEnabled && (
            <Text style={styles.videoHint}>Giữ app mở trong lúc chạy (màn hình sẽ luôn sáng). Quay 480p có tiếng, mỗi đoạn 1 phút.</Text>
          )}
        </View>

        {!tracking ? (
          <TouchableOpacity style={styles.btnPrimary} onPress={startTracking}>
            <Text style={styles.btnText}>Start GPS Tracking</Text>
          </TouchableOpacity>
        ) : (
          <>
            <TouchableOpacity style={styles.btnPrimary} onPress={() => setShowNav(true)}>
              <Text style={styles.btnText}>Open Navigation</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.btnDanger} onPress={stopTracking}>
              <Text style={styles.btnText}>Stop Tracking</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.btnSuccess} onPress={reportArrival}>
              <Text style={styles.btnText}>✅ {arrivalLabel}</Text>
            </TouchableOpacity>
          </>
        )}

        <Text style={styles.note}>
          Once tracking starts, you can switch to other apps and GPS will keep reporting
          every 30 seconds in the background. Tap "Open Navigation" for a map with
          turn-by-turn directions that automatically reroute if you go off path.
        </Text>
      </ScrollView>
    </SafeAreaView>
  );

  // 1 gốc chung cho cả 2 màn hình -> khung camera luôn ở CÙNG vị trí trong cây
  // giao diện, chuyển màn hình không làm camera bị tháo ra/gắn lại (đứt video).
  return (
    <View style={{ flex: 1 }}>
      {showNav ? navScreen : showConfirm ? confirmScreen : mainScreen}
      {cameraOverlay}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f5f5' },
  scroll: { padding: 24, paddingTop: 60 },
  title: { fontSize: 24, fontWeight: 'bold', textAlign: 'center', color: '#2E7D32' },
  subtitle: { fontSize: 14, textAlign: 'center', color: '#888', marginBottom: 24 },
  statusBox: { padding: 16, borderRadius: 10, marginBottom: 24 },
  statusActive: { backgroundColor: '#d4edda' },
  statusInactive: { backgroundColor: '#e2e3e5' },
  statusText: { textAlign: 'center', fontWeight: '600' },
  label: { fontSize: 13, color: '#666', marginBottom: 6, marginTop: 12 },
  input: { borderWidth: 1, borderColor: '#ccc', borderRadius: 8, padding: 12, fontSize: 16, backgroundColor: 'white' },
  btnPrimary: { backgroundColor: '#2E7D32', padding: 16, borderRadius: 10, marginTop: 12, alignItems: 'center' },
  btnDanger: { backgroundColor: '#C62828', padding: 16, borderRadius: 10, marginTop: 12, alignItems: 'center' },
  btnSuccess: { backgroundColor: '#1565C0', padding: 16, borderRadius: 10, marginTop: 12, alignItems: 'center' },
  btnText: { color: 'white', fontWeight: 'bold', fontSize: 14 },
  note: { fontSize: 12, color: '#999', marginTop: 24, textAlign: 'center' },
  btnCloseNav: { backgroundColor: '#C62828', padding: 14, alignItems: 'center' },
  videoCard: { backgroundColor: 'white', borderRadius: 10, padding: 14, marginTop: 20, borderWidth: 1, borderColor: '#e0e0e0' },
  videoRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  videoTitle: { fontSize: 15, fontWeight: 'bold', color: '#333' },
  videoStatus: { fontSize: 13, color: '#555', marginTop: 6 },
  videoError: { fontSize: 12, color: '#C62828', marginTop: 6 },
  btnRetry: { marginTop: 8, alignSelf: 'flex-start', borderWidth: 1, borderColor: '#2E7D32', borderRadius: 6, paddingHorizontal: 10, paddingVertical: 5 },
  btnRetryText: { color: '#2E7D32', fontWeight: 'bold', fontSize: 12 },
  videoHint: { fontSize: 11, color: '#999', marginTop: 6 },
  camBox: {
    position: 'absolute', top: 100, right: 10, width: 108, height: 144, borderRadius: 10,
    overflow: 'hidden', backgroundColor: '#000', borderWidth: 2, borderColor: '#fff', elevation: 6,
  },
  warnBox: { backgroundColor: '#FFF8E1', borderRadius: 10, padding: 12, marginBottom: 12, borderWidth: 1, borderColor: '#FFE082' },
  warnText: { fontSize: 13, color: '#795548' },
  cargoRow: { flexDirection: 'row', backgroundColor: 'white', borderRadius: 10, padding: 12, marginTop: 10, borderWidth: 1, borderColor: '#e0e0e0', alignItems: 'center' },
  cargoRowChecked: { borderColor: '#2E7D32', backgroundColor: '#E8F5E9' },
  cargoCheck: { fontSize: 26, marginRight: 12, color: '#2E7D32' },
  cargoName: { fontSize: 15, fontWeight: 'bold', color: '#333' },
  cargoQty: { fontSize: 14, color: '#1565C0', marginTop: 2, fontWeight: '600' },
  cargoHint: { fontSize: 11, color: '#999', marginTop: 2 },
  photoRow: { flexDirection: 'row', flexWrap: 'wrap', marginTop: 8 },
  photoThumbBox: { marginRight: 8, marginBottom: 8, alignItems: 'center' },
  photoThumb: { width: 72, height: 72, borderRadius: 6, backgroundColor: '#eee' },
  photoState: { fontSize: 10, color: '#2E7D32', marginTop: 2 },
  btnDisabled: { opacity: 0.4 },
  recBadge: { position: 'absolute', top: 4, left: 4, backgroundColor: 'rgba(198,40,40,0.9)', borderRadius: 6, paddingHorizontal: 5, paddingVertical: 1 },
  recBadgeText: { color: 'white', fontSize: 10, fontWeight: 'bold' },
});
