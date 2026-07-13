using Microsoft.UI.Xaml.Controls;
using ServiceLib.ViewModels;

namespace v2rayN.WinUI.Views;

public sealed partial class UpdateManagerView : UserControl
{
    private readonly CheckUpdateViewModel _viewModel;

    public UpdateManagerView(CheckUpdateViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        PrereleaseButton.IsChecked = viewModel.EnableCheckPreReleaseUpdate;
        PrereleaseButton.Click += (_, _) => viewModel.EnableCheckPreReleaseUpdate = PrereleaseButton.IsChecked == true;
    }
}
