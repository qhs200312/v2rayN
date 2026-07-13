using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ServiceLib.Models.Entities;
using ServiceLib.Resx;
using ServiceLib.ViewModels;

namespace v2rayN.WinUI.Views;

public sealed class GroupServerEditorView : UserControl
{
    private readonly AddGroupServerViewModel _viewModel;
    private readonly ListView _previewList;
    private readonly ListView _childList;

    public GroupServerEditorView(AddGroupServerViewModel viewModel)
    {
        _viewModel = viewModel;
        var root = new Grid { RowSpacing = 10 };
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(280) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        var configuration = new Grid { ColumnSpacing = 12 };
        configuration.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        configuration.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        configuration.Children.Add(new DynamicFormView(viewModel));
        var selectors = new StackPanel { Spacing = 10 };
        var core = new ComboBox { Header = "核心类型", ItemsSource = new[] { "Xray", "sing_box" }, SelectedItem = viewModel.CoreType };
        core.SelectionChanged += (_, _) => viewModel.CoreType = core.SelectedItem?.ToString();
        var policy = new ComboBox { Header = "策略类型", ItemsSource = new[] { ResUI.TbLeastPing, ResUI.TbFallback, ResUI.TbRandom, ResUI.TbRoundRobin, ResUI.TbLeastLoad }, SelectedItem = viewModel.PolicyGroupType };
        policy.SelectionChanged += (_, _) => viewModel.PolicyGroupType = policy.SelectedItem?.ToString();
        var subscription = new ComboBox { Header = "动态订阅子节点", ItemsSource = viewModel.SubItems, DisplayMemberPath = "Remarks", SelectedItem = viewModel.SelectedSubItem };
        subscription.SelectionChanged += (_, _) => viewModel.SelectedSubItem = subscription.SelectedItem as ServiceLib.Models.Entities.SubItem;
        var filter = new TextBox { Header = "节点筛选", Text = viewModel.Filter ?? string.Empty };
        filter.TextChanged += (_, _) => viewModel.Filter = filter.Text;
        selectors.Children.Add(core);
        selectors.Children.Add(policy);
        selectors.Children.Add(subscription);
        selectors.Children.Add(filter);
        Grid.SetColumn(selectors, 1);
        configuration.Children.Add(selectors);
        root.Children.Add(configuration);

        var bar = new CommandBar { DefaultLabelPosition = CommandBarDefaultLabelPosition.Right, IsOpen = true, IsDynamicOverflowEnabled = false };
        var refresh = new AppBarButton { Label = "刷新候选节点", Icon = new SymbolIcon(Symbol.Refresh) };
        refresh.Click += async (_, _) => await viewModel.UpdatePreviewList();
        var add = new AppBarButton { Label = "加入所选节点", Icon = new SymbolIcon(Symbol.Add) };
        add.Click += (_, _) => AddSelected();
        bar.PrimaryCommands.Add(refresh);
        bar.PrimaryCommands.Add(add);
        bar.PrimaryCommands.Add(new AppBarButton { Label = "移除", Icon = new SymbolIcon(Symbol.Delete), Command = viewModel.RemoveCmd });
        bar.PrimaryCommands.Add(new AppBarButton { Label = "置顶", Icon = new SymbolIcon(Symbol.Upload), Command = viewModel.MoveTopCmd });
        bar.PrimaryCommands.Add(new AppBarButton { Label = "上移", Icon = new SymbolIcon(Symbol.Up), Command = viewModel.MoveUpCmd });
        bar.PrimaryCommands.Add(new AppBarButton { Label = "下移", Icon = new SymbolIcon(Symbol.Download), Command = viewModel.MoveDownCmd });
        bar.PrimaryCommands.Add(new AppBarButton { Label = "置底", Icon = new SymbolIcon(Symbol.Download), Command = viewModel.MoveBottomCmd });
        Grid.SetRow(bar, 1);
        root.Children.Add(bar);

        var lists = new Grid { ColumnSpacing = 12 };
        lists.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        lists.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _previewList = new ListView { Header = "候选节点", ItemsSource = viewModel.AllProfilePreviewItemsObs, DisplayMemberPath = "Remarks", SelectionMode = ListViewSelectionMode.Extended };
        _childList = new ListView { Header = "已选子节点", ItemsSource = viewModel.ChildItemsObs, DisplayMemberPath = "Remarks", SelectionMode = ListViewSelectionMode.Extended };
        _childList.SelectionChanged += (_, _) =>
        {
            viewModel.SelectedChild = _childList.SelectedItem as ProfileItem ?? new ProfileItem();
            viewModel.SelectedChildren = _childList.SelectedItems.Cast<ProfileItem>().ToList();
        };
        lists.Children.Add(_previewList);
        Grid.SetColumn(_childList, 1);
        lists.Children.Add(_childList);
        Grid.SetRow(lists, 2);
        root.Children.Add(lists);
        Content = root;
    }

    private void AddSelected()
    {
        foreach (var profile in _previewList.SelectedItems.Cast<ProfileItem>())
        {
            if (_viewModel.ChildItemsObs.All(item => item.IndexId != profile.IndexId))
            {
                _viewModel.ChildItemsObs.Add(profile);
            }
        }
    }
}
