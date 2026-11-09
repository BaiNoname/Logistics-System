namespace TruckingSystem.Desktop.Common
{
    /// <summary>NHỚ BỘ LỌC của từng màn trong lúc dùng app: lọc "Đang vận chuyển" ở màn Đơn, chuyển sang màn khác rồi quay
    /// lại thì bộ lọc + ô tìm vẫn giữ nguyên (trước đây mỗi lần mở lại màn là lọc lại từ đầu). Đóng app thì quên.
    /// Mở màn từ thông báo / ô tìm nhanh (đã lọc sẵn đúng 1 dòng) thì KHÔNG khôi phục đè lên.
    /// Dùng: FilterMemory.Attach(filterBar, "orders"); — gọi trong constructor của màn, sau khi AddChoice.
    /// Không sửa FilterBar: chỉ đọc / đặt các ô chọn + ô tìm có sẵn bên trong nó.</summary>
    public static class FilterMemory
    {
        private sealed record State(string Search, string?[] Choices);
        private static readonly Dictionary<string, State> Saved = new();
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<FilterBar, object> Cancelled = new();

        /// <summary>Bỏ khôi phục bộ lọc cũ cho lần mở này (VD mở màn từ số đỏ trên menu: chỉ hiện việc chờ) và xóa các ô lọc.</summary>
        public static void Cancel(FilterBar bar)
        {
            Cancelled.AddOrUpdate(bar, new object());
            var search = FindAll<TextBox>(bar).FirstOrDefault();
            if (search is not null && search.Text.Length > 0) search.Text = "";
            foreach (var c in FindAll<ComboBox>(bar)) if (c.Items.Count > 0 && c.SelectedIndex != 0) c.SelectedIndex = 0;
        }

        public static void Attach(FilterBar bar, string key)
        {
            var search = FindAll<TextBox>(bar).FirstOrDefault();

            // Lưu lại MỖI KHI bộ lọc đổi (lúc màn bị đóng thì các ô đã bị giải phóng, không đọc được nữa).
            bar.FilterChanged += (_, _) =>
            {
                if (bar.IsDisposed) return;
                Saved[key] = new State(search?.Text ?? "",
                    FindAll<ComboBox>(bar).Select(c => c.SelectedIndex > 0 ? c.SelectedItem as string : null).ToArray());
            };

            if (!Saved.TryGetValue(key, out var state)) return;
            if (search is not null && state.Search.Length > 0) search.Text = state.Search;

            // Danh sách lựa chọn của nhiều ô chỉ có sau khi màn tải xong dữ liệu -> thử lại vài lần trong ~8 giây.
            var pending = state.Choices.Select((v, i) => (Index: i, Value: v)).Where(x => x.Value is not null).ToList();
            if (pending.Count == 0) return;
            var timer = new System.Windows.Forms.Timer { Interval = 250 };
            var tries = 0;
            timer.Tick += (_, _) =>
            {
                // Màn đã đóng, quá lâu, hoặc vừa được mở từ thông báo / ô tìm (ô tìm đã bị đổi) -> thôi.
                if (bar.IsDisposed || ++tries > 32 || Cancelled.TryGetValue(bar, out _) || (search is not null && search.Text != state.Search))
                {
                    timer.Dispose();
                    return;
                }
                var combos = FindAll<ComboBox>(bar).ToList();
                foreach (var p in pending.ToList())
                {
                    if (p.Index >= combos.Count) { pending.Remove(p); continue; }
                    var c = combos[p.Index];
                    var at = c.Items.IndexOf(p.Value!);
                    if (at > 0) { c.SelectedIndex = at; pending.Remove(p); }
                }
                if (pending.Count == 0) timer.Dispose();
            };
            timer.Start();
        }

        private static IEnumerable<T> FindAll<T>(Control root) where T : Control
        {
            foreach (Control c in root.Controls)
            {
                if (c is T t) yield return t;
                foreach (var inner in FindAll<T>(c)) yield return inner;
            }
        }
    }
}
