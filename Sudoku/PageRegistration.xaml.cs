using System;
using System.Text.RegularExpressions; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Sudoku
{
    /// <summary>
    /// Логика взаимодействия для PageRegistration.xaml
    /// </summary>
    public partial class PageRegistration : Page
    {
        public PageRegistration()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is Window window)
            {
                window.Title = "Registration";
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            PageAccount accountPage = new PageAccount();
            this.NavigationService.Navigate(accountPage);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string password = PassTextBox.Password;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Пожалуйста, введите имя пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidatePassword(password, out string errorMsg))
            {
                MessageBox.Show(errorMsg, "Недопустимый пароль", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Database.RegisterUser(name, password, out string dbError, false))
            {
                MessageBox.Show("Регистрация прошла успешно! Теперь войдите в аккаунт.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                PageAccount accountPage = new PageAccount();
                this.NavigationService.Navigate(accountPage);
            }
            else
            {
                MessageBox.Show($"Не удалось зарегистрироваться:\n{dbError}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidatePassword(string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            // 1. Длина от 8 до 32 символов
            if (password.Length < 8 || password.Length > 32)
            {
                errorMessage = "Пароль должен быть длиной от 8 до 32 символов.";
                return false;
            }

            // 2. Только латинские буквы и цифры 
            if (!Regex.IsMatch(password, "^[a-zA-Z0-9]+$"))
            {
                errorMessage = "Пароль может содержать только латинские буквы и цифры.";
                return false;
            }

            // 3. Минимум одна заглавная буква
            if (!Regex.IsMatch(password, "[A-Z]"))
            {
                errorMessage = "Пароль должен содержать хотя бы одну заглавную букву (A-Z).";
                return false;
            }

            // 4. Минимум одна строчная буква
            if (!Regex.IsMatch(password, "[a-z]"))
            {
                errorMessage = "Пароль должен содержать хотя бы одну строчную букву (a-z).";
                return false;
            }

            // 5. Минимум одна цифра
            if (!Regex.IsMatch(password, "[0-9]"))
            {
                errorMessage = "Пароль должен содержать хотя бы одну цифру.";
                return false;
            }

            return true;
        }
    }
}