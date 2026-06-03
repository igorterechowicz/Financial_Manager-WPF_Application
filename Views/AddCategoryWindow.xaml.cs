using System.Windows;
using wpf_projekt.Data;
using wpf_projekt.Repositories;
using wpf_projekt.ViewModels;

namespace wpf_projekt.Views
{
    public partial class AddCategoryWindow : Window
    {
        public AddCategoryWindow()
        {
            InitializeComponent();

            var context = new AppDbContext();
            var categoryRepo = new CategoryRepository(context);
            var vm = new AddCategoryViewModel(categoryRepo);

            // Subskrypcja zdarzeń zamiast bezpośredniego zamykania w VM
            vm.SavedSuccessfully += () =>
            {
                DialogResult = true;
                Close();
            };
            vm.Cancelled += () =>
            {
                DialogResult = false;
                Close();
            };

            DataContext = vm;
            CategoryNameTextBox.Focus();
        }
    }
}