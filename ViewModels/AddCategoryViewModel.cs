using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using wpf_projekt.Entities;
using wpf_projekt.Repositories;

namespace wpf_projekt.ViewModels
{
    public partial class AddCategoryViewModel : ObservableObject
    {
        private readonly ICategoryRepository _categoryRepository;

        [ObservableProperty] private string _categoryName = string.Empty;

        /// <summary>
        /// Ustawiane na true gdy zapis się powiódł.
        /// Widok subskrybuje to zdarzenie i zamyka okno z DialogResult = true.
        /// </summary>
        public event Action? SavedSuccessfully;
        public event Action? Cancelled;

        public AddCategoryViewModel(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task SaveAsync()
        {
            var name = CategoryName.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Nazwa kategorii nie może być pusta!", "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _categoryRepository.AddAsync(new TransactionType { Name = name });
                SavedSuccessfully?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas zapisu: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Cancel() => Cancelled?.Invoke();
    }
}