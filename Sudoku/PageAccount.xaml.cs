using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Sudoku
{
    /// <summary>
    /// Логика взаимодействия для PageAccount.xaml (Страница Входа)
    /// </summary>
    public partial class PageAccount : Page
    {
        public PageAccount()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string password = PassTextBox.Password;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите имя и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Database.AuthenticateUser(name, password))
            {
                UserSession.CurrentUserName = name;

                MessageBox.Show($"Добро пожаловать, {name}!", "Вход выполнен", MessageBoxButton.OK, MessageBoxImage.Information);

                PageMainMenu mainMenuPage = new PageMainMenu();
                this.NavigationService.Navigate(mainMenuPage);
            }
            else
            {
                MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            PageRegistration registrationPage = new PageRegistration();
            this.NavigationService.Navigate(registrationPage);
        }
    }
}