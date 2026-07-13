using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ServiceLib.Enums;
using ServiceLib.Models.Configs;
using ServiceLib.ViewModels;

namespace v2rayN.WinUI.Views;

public sealed class HotkeyEditorView : UserControl
{
    private readonly GlobalHotkeySettingViewModel _viewModel;
    private readonly Action _saved;
    private readonly List<HotkeyOption> _keyOptions = BuildKeyOptions();

    public HotkeyEditorView(GlobalHotkeySettingViewModel viewModel, Action saved)
    {
        _viewModel = viewModel;
        _saved = saved;
        var root = new Grid { RowSpacing = 10, MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var labels = new[] { "显示主窗口", "清除系统代理", "自动配置系统代理", "不改变系统代理", "PAC 模式" };
        for (var index = 0; index < labels.Length; index++)
        {
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var label = new TextBlock { Text = labels[index], VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(label, index);
            root.Children.Add(label);
            var editor = BuildEditor((EGlobalHotkey)index);
            Grid.SetRow(editor, index);
            Grid.SetColumn(editor, 1);
            root.Children.Add(editor);
        }

        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 16, 0, 0) };
        var reset = new Button { Content = "重置" };
        reset.Click += (_, _) =>
        {
            _viewModel.ResetKeyEventItem();
            Content = new HotkeyEditorView(_viewModel, _saved).Content;
        };
        var save = new Button { Content = "保存", Command = _viewModel.SaveCmd };
        save.Click += (_, _) => _saved();
        buttons.Children.Add(reset);
        buttons.Children.Add(save);
        Grid.SetRow(buttons, labels.Length);
        Grid.SetColumn(buttons, 1);
        root.Children.Add(buttons);
        Content = root;
    }

    private FrameworkElement BuildEditor(EGlobalHotkey type)
    {
        var item = _viewModel.GetKeyEventItem(type);
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var ctrl = new CheckBox { Content = "Ctrl", IsChecked = item.Control };
        var alt = new CheckBox { Content = "Alt", IsChecked = item.Alt };
        var shift = new CheckBox { Content = "Shift", IsChecked = item.Shift };
        var key = new ComboBox { Width = 180, ItemsSource = _keyOptions, DisplayMemberPath = "Name" };
        key.SelectedItem = _keyOptions.FirstOrDefault(option => option.WpfKeyCode == item.KeyCode) ?? _keyOptions[0];
        ctrl.Click += (_, _) => item.Control = ctrl.IsChecked == true;
        alt.Click += (_, _) => item.Alt = alt.IsChecked == true;
        shift.Click += (_, _) => item.Shift = shift.IsChecked == true;
        key.SelectionChanged += (_, _) => item.KeyCode = (key.SelectedItem as HotkeyOption)?.WpfKeyCode;
        panel.Children.Add(ctrl);
        panel.Children.Add(alt);
        panel.Children.Add(shift);
        panel.Children.Add(key);
        return panel;
    }

    private static List<HotkeyOption> BuildKeyOptions()
    {
        var options = new List<HotkeyOption> { new("无", 0) };
        for (var i = 0; i < 26; i++)
        {
            options.Add(new(((char)('A' + i)).ToString(), 44 + i));
        }

        for (var i = 0; i < 10; i++)
        {
            options.Add(new(i.ToString(), 34 + i));
        }

        for (var i = 0; i < 12; i++)
        {
            options.Add(new($"F{i + 1}", 90 + i));
        }

        options.AddRange([new("空格", 18), new("Home", 22), new("End", 21), new("PageUp", 19), new("PageDown", 20), new("Insert", 31), new("Delete", 32)]);
        return options;
    }

    private sealed record HotkeyOption(string Name, int WpfKeyCode);
}
