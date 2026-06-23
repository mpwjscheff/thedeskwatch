using CommunityToolkit.Maui.Views;
using TheDeskWatch.MobileApp.Pages.Colleagues.ViewModels;

namespace TheDeskWatch.MobileApp.Pages.Colleagues.Views;

public partial class ColleagueFormPopup : Popup
{
    public ColleagueFormPopup(ColleagueFormViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
        viewModel.SaveCompleted += async (_, _) => await CloseAsync();
    }

    public void PrepareForAdd()
        => ((ColleagueFormViewModel)BindingContext).Initialize(null, null, null);

    public void PrepareForEdit(int id, string name, string hexColor)
        => ((ColleagueFormViewModel)BindingContext).Initialize(id, name, hexColor);
}
