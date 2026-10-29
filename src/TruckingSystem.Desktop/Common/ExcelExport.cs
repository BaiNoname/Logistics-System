using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>XUẤT EXCEL (.xlsx thật, mở được bằng Excel / Google Sheets / LibreOffice) — tự ghi file theo chuẩn
    /// Office Open XML, KHÔNG cần cài thêm thư viện NuGet nào.
    ///  • Dòng tiêu đề in đậm nền tối, cố định khi cuộn (freeze), có sẵn nút lọc (AutoFilter).
    ///  • Cột tự giãn theo nội dung; số giữ kiểu SỐ (cộng / lọc được trong Excel), còn lại là chữ.
    /// Dùng: ExcelExport.ExportGrid(FindForm(), grid, "Don van chuyen", "DonVanChuyen") — xuất đúng các cột / dòng đang
    /// HIỆN trên bảng (đã lọc bao nhiêu thì xuất bấy nhiêu), hoặc Export(...) với dữ liệu tự chọn.</summary>
    public static class ExcelExport
    {
        /// <summary>Xuất các cột đang hiện + các dòng đang hiện của bảng (giá trị như đang thấy trên màn hình).</summary>
        public static void ExportGrid(IWin32Window? owner, DataGridView grid, string sheetName, string fileName)
        {
            var columns = grid.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Visible)
                .OrderBy(c => c.DisplayIndex)
                .ToList();
            var headers = columns.Select(c => (c.HeaderText ?? c.Name).Replace('_', ' ').Trim()).ToList();
            var rows = grid.Rows.Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow && r.Visible)
                .Select(r => columns.Select(c =>
                {
                    var cell = r.Cells[c.Index];
                    return IsNumber(cell.Value) ? cell.Value : cell.FormattedValue?.ToString() ?? cell.Value?.ToString();
                }).ToArray())
                .ToList();
            Export(owner, sheetName, fileName, headers, rows);
        }

        /// <summary>Hỏi nơi lưu rồi ghi file .xlsx; xong hỏi có mở file luôn không.</summary>
        public static void Export(IWin32Window? owner, string sheetName, string fileName,
            IReadOnlyList<string> headers, IReadOnlyCollection<object?[]> rows)
        {
            if (rows.Count == 0)
            {
                MessageBox.Show(owner, "Không có dòng nào để xuất (bảng đang trống hoặc bộ lọc không khớp dòng nào).",
                    "Xuất Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Title = "Xuất Excel",
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = $"{fileName}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                OverwritePrompt = true,
            };
            if (dialog.ShowDialog(owner) != DialogResult.OK) return;

            try
            {
                Write(dialog.FileName, sheetName, headers, rows);
            }
            catch (IOException ex)
            {
                MessageBox.Show(owner, "Không ghi được file — nếu file đang mở trong Excel, hãy đóng lại rồi thử lại.\n\n" + ex.Message,
                    "Xuất Excel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var open = MessageBox.Show(owner, $"Đã xuất {rows.Count} dòng ra file:\n{dialog.FileName}\n\nMở file ngay?",
                "Xuất Excel", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (open == DialogResult.Yes)
            {
                try { Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true }); }
                catch { /* máy chưa cài chương trình mở .xlsx — file vẫn đã lưu */ }
            }
        }

        // ------------------------------------------------------------------ ghi file .xlsx

        public static void Write(string path, string sheetName, IReadOnlyList<string> headers, IEnumerable<object?[]> rows)
        {
            var rowList = rows.ToList();
            var widths = headers.Select(h => (double)Math.Max(8, h.Length + 4)).ToArray();

            var sheet = new StringBuilder();
            sheet.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sheet.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
            sheet.Append("<sheetViews><sheetView workbookViewId=\"0\"><pane ySplit=\"1\" topLeftCell=\"A2\" activePane=\"bottomLeft\" state=\"frozen\"/></sheetView></sheetViews>");

            var data = new StringBuilder();
            data.Append("<sheetData>");
            data.Append("<row r=\"1\">");
            for (var c = 0; c < headers.Count; c++)
                data.Append(TextCell(Ref(c, 1), headers[c], style: 1));
            data.Append("</row>");
            for (var r = 0; r < rowList.Count; r++)
            {
                var rowNo = r + 2;
                data.Append("<row r=\"").Append(rowNo).Append("\">");
                var values = rowList[r];
                for (var c = 0; c < headers.Count; c++)
                {
                    var v = c < values.Length ? values[c] : null;
                    if (v is null) continue;
                    if (IsNumber(v))
                    {
                        var num = Convert.ToDecimal(v, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                        data.Append("<c r=\"").Append(Ref(c, rowNo)).Append("\"><v>").Append(num).Append("</v></c>");
                        widths[c] = Math.Max(widths[c], num.Length + 3);
                    }
                    else
                    {
                        var text = v is DateTime d ? d.ToString(d.TimeOfDay == TimeSpan.Zero ? "dd/MM/yyyy" : "dd/MM/yyyy HH:mm") : v.ToString() ?? "";
                        if (text.Length == 0) continue;
                        data.Append(TextCell(Ref(c, rowNo), text, style: 0));
                        var longestLine = text.Split('\n').Max(l => l.Length);
                        widths[c] = Math.Max(widths[c], longestLine + 2);
                    }
                }
                data.Append("</row>");
            }
            data.Append("</sheetData>");

            sheet.Append("<cols>");
            for (var c = 0; c < widths.Length; c++)
                sheet.Append($"<col min=\"{c + 1}\" max=\"{c + 1}\" width=\"{Math.Min(widths[c], 70).ToString("0.#", CultureInfo.InvariantCulture)}\" customWidth=\"1\"/>");
            sheet.Append("</cols>");
            sheet.Append(data);
            sheet.Append($"<autoFilter ref=\"A1:{Ref(Math.Max(0, headers.Count - 1), rowList.Count + 1)}\"/>");
            sheet.Append("</worksheet>");

            if (File.Exists(path)) File.Delete(path);
            using var zip = ZipFile.Open(path, ZipArchiveMode.Create);
            Add(zip, "[Content_Types].xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>" +
                "</Types>");
            Add(zip, "_rels/.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                "</Relationships>");
            Add(zip, "xl/workbook.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                $"<sheets><sheet name=\"{Escape(SafeSheetName(sheetName))}\" sheetId=\"1\" r:id=\"rId1\"/></sheets>" +
                $"<definedNames><definedName name=\"_xlnm._FilterDatabase\" localSheetId=\"0\" hidden=\"1\">'{Escape(SafeSheetName(sheetName))}'!$A$1:${Col(Math.Max(0, headers.Count - 1))}${rowList.Count + 1}</definedName></definedNames>" +
                "</workbook>");
            Add(zip, "xl/_rels/workbook.xml.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>" +
                "</Relationships>");
            // Kiểu 0: chữ thường (canh trên, tự xuống dòng); kiểu 1: tiêu đề đậm chữ trắng nền xanh đậm.
            Add(zip, "xl/styles.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
                "<fonts count=\"2\"><font><sz val=\"11\"/><name val=\"Calibri\"/></font>" +
                "<font><b/><sz val=\"11\"/><color rgb=\"FFFFFFFF\"/><name val=\"Calibri\"/></font></fonts>" +
                "<fills count=\"3\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill>" +
                "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FF1E293B\"/><bgColor indexed=\"64\"/></patternFill></fill></fills>" +
                "<borders count=\"1\"><border><left/><right/><top/><bottom/><diagonal/></border></borders>" +
                "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>" +
                "<cellXfs count=\"2\">" +
                "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyAlignment=\"1\"><alignment vertical=\"top\" wrapText=\"1\"/></xf>" +
                "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyAlignment=\"1\"><alignment vertical=\"center\"/></xf>" +
                "</cellXfs>" +
                "<cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\"/></cellStyles>" +
                "</styleSheet>");
            Add(zip, "xl/worksheets/sheet1.xml", sheet.ToString());
        }

        private static bool IsNumber(object? v)
            => v is int or long or short or byte or decimal or double or float;

        private static string TextCell(string cellRef, string text, int style)
            => $"<c r=\"{cellRef}\" t=\"inlineStr\" s=\"{style}\"><is><t xml:space=\"preserve\">{Escape(text)}</t></is></c>";

        private static string Ref(int col, int row) => Col(col) + row.ToString(CultureInfo.InvariantCulture);

        private static string Col(int index)
        {
            var name = "";
            index++;
            while (index > 0)
            {
                var m = (index - 1) % 26;
                name = (char)('A' + m) + name;
                index = (index - 1) / 26;
            }
            return name;
        }

        private static string SafeSheetName(string name)
        {
            var cleaned = new string(name.Where(ch => "[]:*?/\\'".IndexOf(ch) < 0).ToArray()).Trim();
            if (cleaned.Length == 0) cleaned = "Sheet1";
            return cleaned.Length > 31 ? cleaned[..31] : cleaned;
        }

        /// <summary>Thoát ký tự XML + bỏ ký tự điều khiển không hợp lệ trong XML (trừ tab / xuống dòng).</summary>
        private static string Escape(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                switch (ch)
                {
                    case '&': sb.Append("&amp;"); break;
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    case '"': sb.Append("&quot;"); break;
                    case '\r': break;
                    default:
                        if (ch < 0x20 && ch != '\t' && ch != '\n') break;
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }

        private static void Add(ZipArchive zip, string name, string content)
        {
            var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
            using var stream = entry.Open();
            var bytes = new UTF8Encoding(false).GetBytes(content);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
