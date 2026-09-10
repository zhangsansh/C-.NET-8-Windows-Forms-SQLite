namespace BankManagementSystem.Utils;

/// <summary>客户/产品页共用的模糊、联合、分级查询辅助。</summary>
public static class SearchHelper
{
    public enum MatchMode
    {
        Fuzzy = 0,   // 模糊包含
        Exact = 1,   // 精确匹配
        Prefix = 2   // 前缀匹配（分级定位）
    }

    /// <summary>
    /// 模糊匹配：空格分隔为 AND；用 | 或 、 分隔为 OR 组。
    /// 例：张 北京 → 同时包含；张三|李四 → 任一即可。
    /// </summary>
    public static bool MatchText(string? source, string? keyword, MatchMode mode = MatchMode.Fuzzy)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return true;
        var text = source ?? string.Empty;
        var raw = keyword.Trim();

        // OR 组
        var orParts = raw.Split(new[] { '|', '、' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (orParts.Length > 1)
            return orParts.Any(part => MatchAndGroup(text, part, mode));

        return MatchAndGroup(text, raw, mode);
    }

    private static bool MatchAndGroup(string text, string group, MatchMode mode)
    {
        var andParts = group.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (andParts.Length == 0) return true;
        return andParts.All(token => MatchToken(text, token, mode));
    }

    private static bool MatchToken(string text, string token, MatchMode mode) => mode switch
    {
        MatchMode.Exact => text.Equals(token, StringComparison.OrdinalIgnoreCase),
        MatchMode.Prefix => text.StartsWith(token, StringComparison.OrdinalIgnoreCase),
        _ => text.Contains(token, StringComparison.OrdinalIgnoreCase)
    };

    public static bool InDateRange(DateTime value, DateTime? fromInclusive, DateTime? toInclusive)
    {
        if (fromInclusive.HasValue && value.Date < fromInclusive.Value.Date) return false;
        if (toInclusive.HasValue && value.Date > toInclusive.Value.Date) return false;
        return true;
    }

    public static bool InDecimalRange(decimal value, decimal? min, decimal? max)
    {
        if (min.HasValue && value < min.Value) return false;
        if (max.HasValue && value > max.Value) return false;
        return true;
    }

    public static decimal? TryParseDecimal(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        return decimal.TryParse(text.Trim(), out var v) ? v : null;
    }

    public static DateTimePicker CreateDatePicker(bool endOfDayHint = false)
    {
        return new DateTimePicker
        {
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "yyyy-MM-dd",
            Width = 150,
            MinimumSize = new Size(150, 28),
            ShowCheckBox = true,
            Checked = false,
            Margin = new Padding(4, 4, 10, 4)
        };
    }

    public static ComboBox CreateCombo(int width, params string[] items)
    {
        var c = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = width,
            MinimumSize = new Size(width, 28),
            DropDownWidth = Math.Max(width, 160),
            Margin = new Padding(4, 4, 10, 4)
        };
        c.Items.AddRange(items);
        if (c.Items.Count > 0) c.SelectedIndex = 0;
        // 下拉列表按最长项加宽，避免文字显示不全
        try
        {
            using var g = c.CreateGraphics();
            var max = width;
            foreach (var item in items)
            {
                var w = TextRenderer.MeasureText(g, item, c.Font).Width + 28;
                if (w > max) max = w;
            }
            c.DropDownWidth = Math.Min(Math.Max(max, width), 420);
        }
        catch { /* 设计时/未创建句柄时忽略 */ }
        return c;
    }

    public static Label CreateLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(6, 10, 4, 4),
        ForeColor = UiTheme.TextMuted
    };

    public static TextBox CreateBox(int width, string? placeholder = null)
    {
        var t = new TextBox
        {
            Width = width,
            MinimumSize = new Size(width, 28),
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Margin = new Padding(4, 4, 10, 4)
        };
        if (!string.IsNullOrEmpty(placeholder))
            t.PlaceholderText = placeholder;
        return t;
    }

    /// <summary>可完整展示的筛选面板：按内容自动增高，窄屏时内部可滚动，避免被裁切。</summary>
    public static Panel CreateFilterPanel(Control content)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            BackColor = Color.White,
            Padding = new Padding(14, 10, 14, 10),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            AutoScroll = true,
            MinimumSize = new Size(0, 96)
        };
        panel.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(220, 225, 230));
            e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        };
        content.Dock = DockStyle.Top;
        content.AutoSize = true;
        if (content is Panel p)
            p.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        panel.Controls.Add(content);
        void SyncWidth()
        {
            // 尽量铺满整页宽度，便于搜索区横向展开
            var w = Math.Max(panel.ClientSize.Width - 8, 980);
            if (content.Width != w) content.Width = w;
        }
        panel.Resize += (_, _) => SyncWidth();
        panel.Layout += (_, _) => SyncWidth();
        return panel;
    }

    /// <summary>筛选区一行：自动换行并按内容增高，保证控件不被遮挡。</summary>
    public static FlowLayoutPanel CreateFilterRow() => new()
    {
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        WrapContents = true,
        FlowDirection = FlowDirection.LeftToRight,
        Margin = new Padding(0, 0, 0, 6),
        Padding = new Padding(0, 2, 0, 2),
        Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
        MinimumSize = new Size(960, 0)
    };
}
