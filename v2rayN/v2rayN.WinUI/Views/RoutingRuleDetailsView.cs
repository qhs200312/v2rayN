using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using ServiceLib;
using ServiceLib.Common;
using ServiceLib.Enums;
using ServiceLib.ViewModels;

namespace v2rayN.WinUI.Views;

public sealed class RoutingRuleDetailsView : UserControl
{
    private readonly RoutingRuleDetailsViewModel _viewModel;

    public RoutingRuleDetailsView(RoutingRuleDetailsViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.ProtocolItems = _viewModel.SelectedSource.Protocol?.ToList() ?? [];
        _viewModel.InboundTagItems = _viewModel.SelectedSource.InboundTag?.ToList() ?? [];

        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var saveBar = new CommandBar
        {
            DefaultLabelPosition = CommandBarDefaultLabelPosition.Right,
            IsOpen = true,
            IsDynamicOverflowEnabled = true,
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent)
        };
        saveBar.PrimaryCommands.Add(new AppBarButton
        {
            Label = "保存规则",
            Icon = new SymbolIcon(Symbol.Save),
            Command = _viewModel.SaveCmd
        });
        root.Children.Add(saveBar);

        var form = new StackPanel { Spacing = 12, Padding = new Thickness(8, 4, 16, 24) };
        form.Children.Add(BuildGeneralFields());
        form.Children.Add(BuildSelectionFields());
        form.Children.Add(BuildMatchFields());

        var autoSort = new ToggleSwitch { Header = "保存时自动排序规则内容" };
        autoSort.SetBinding(ToggleSwitch.IsOnProperty, Bind(_viewModel, nameof(_viewModel.AutoSort), BindingMode.TwoWay));
        form.Children.Add(autoSort);

        var scroll = new ScrollViewer
        {
            Content = form,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalContentAlignment = HorizontalAlignment.Stretch
        };
        Grid.SetRow(scroll, 1);
        root.Children.Add(scroll);
        Content = root;
    }

    private Grid BuildGeneralFields()
    {
        var source = _viewModel.SelectedSource;
        var grid = TwoColumnGrid(3);
        AddField(grid, TextField("备注", source, nameof(source.Remarks)), 0, 0);

        var enabled = new ToggleSwitch { Header = "启用规则" };
        enabled.SetBinding(ToggleSwitch.IsOnProperty, Bind(source, nameof(source.Enabled), BindingMode.TwoWay));
        AddField(grid, enabled, 0, 1);

        var ruleType = ComboField("规则类型", Utils.GetEnumNames<ERuleType>().Append(string.Empty), true);
        ruleType.SetBinding(ComboBox.TextProperty, Bind(_viewModel, nameof(_viewModel.RuleType), BindingMode.TwoWay));
        AddField(grid, ruleType, 1, 0);

        var outbound = ComboField("出站标签", Global.OutboundTags, true);
        outbound.SetBinding(ComboBox.TextProperty, Bind(source, nameof(source.OutboundTag), BindingMode.TwoWay));
        AddField(grid, outbound, 1, 1);

        AddField(grid, TextField("端口", source, nameof(source.Port)), 2, 0);
        var network = ComboField("网络", Global.RuleNetworks, true);
        network.SetBinding(ComboBox.TextProperty, Bind(source, nameof(source.Network), BindingMode.TwoWay));
        AddField(grid, network, 2, 1);
        return grid;
    }

    private Grid BuildSelectionFields()
    {
        var grid = TwoColumnGrid(1);
        var protocols = MultiSelect("协议", Global.RuleProtocols, _viewModel.ProtocolItems);
        protocols.SelectionChanged += (_, _) => _viewModel.ProtocolItems = protocols.SelectedItems.Cast<string>().ToList();
        AddField(grid, protocols, 0, 0);

        var inboundTags = MultiSelect("入站标签", Global.InboundTags, _viewModel.InboundTagItems);
        inboundTags.SelectionChanged += (_, _) => _viewModel.InboundTagItems = inboundTags.SelectedItems.Cast<string>().ToList();
        AddField(grid, inboundTags, 0, 1);
        return grid;
    }

    private Grid BuildMatchFields()
    {
        var grid = new Grid { ColumnSpacing = 12 };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        AddField(grid, MultilineField("域名规则", _viewModel, nameof(_viewModel.Domain)), 0, 0);
        AddField(grid, MultilineField("IP 规则", _viewModel, nameof(_viewModel.IP)), 0, 1);
        AddField(grid, MultilineField("进程规则", _viewModel, nameof(_viewModel.Process)), 0, 2);
        return grid;
    }

    private static ListView MultiSelect(string header, IEnumerable<string> items, IList<string> selected)
    {
        var list = new ListView
        {
            Header = header,
            ItemsSource = items,
            SelectionMode = ListViewSelectionMode.Multiple,
            Height = 116
        };
        list.Loaded += (_, _) =>
        {
            foreach (var item in selected.Where(items.Contains))
            {
                list.SelectedItems.Add(item);
            }
        };
        return list;
    }

    private static Grid TwoColumnGrid(int rows)
    {
        var grid = new Grid { RowSpacing = 8, ColumnSpacing = 12 };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        for (var i = 0; i < rows; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        return grid;
    }

    private static TextBox TextField(string header, object source, string property)
    {
        var textBox = new TextBox { Header = header, HorizontalAlignment = HorizontalAlignment.Stretch };
        textBox.SetBinding(TextBox.TextProperty, Bind(source, property, BindingMode.TwoWay));
        return textBox;
    }

    private static TextBox MultilineField(string header, object source, string property)
    {
        var textBox = TextField(header, source, property);
        textBox.AcceptsReturn = true;
        textBox.TextWrapping = TextWrapping.Wrap;
        textBox.Height = 132;
        textBox.FontFamily = new FontFamily("Cascadia Mono");
        return textBox;
    }

    private static ComboBox ComboField(string header, object items, bool editable)
    {
        return new()
        {
            Header = header,
            ItemsSource = items,
            IsEditable = editable,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
    }

    private static Binding Bind(object source, string property, BindingMode mode)
    {
        return new()
        {
            Source = source,
            Path = new PropertyPath(property),
            Mode = mode,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
    }

    private static void AddField(Grid grid, FrameworkElement field, int row, int column)
    {
        Grid.SetRow(field, row);
        Grid.SetColumn(field, column);
        grid.Children.Add(field);
    }
}
