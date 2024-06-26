﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using ToolsManagerApp.Models;
using ToolsManagerApp.Repositories;
using ToolsManagerApp.Services;

namespace ToolsManagerApp.ViewModels
{
    public class UsersViewModel : ObservableObject
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersViewModel> _logger;

        public UsersViewModel() { }

        public UsersViewModel(IUserRepository userRepository, ILogger<UsersViewModel> logger)
        {
            _userRepository = userRepository;
            _logger = logger;

            LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
            AddUserCommand = new AsyncRelayCommand(AddUserAsync);
            UpdateUserCommand = new AsyncRelayCommand(UpdateUserAsync);
            DeleteUserCommand = new AsyncRelayCommand(DeleteUserAsync);
            UnselectUserCommand = new RelayCommand(UnselectUser);

            Users = new ObservableCollection<User>();

            LoadUsersCommand.Execute(null);
        }

        public ObservableCollection<User> Users { get; }

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                if (value != null)
                {
                    NewUserName = value.Name;
                    NewUserEmail = value.Email;
                }
            }
        }

        private string _newUserName;
        public string NewUserName
        {
            get => _newUserName;
            set => SetProperty(ref _newUserName, value);
        }

        private string _newUserEmail;
        public string NewUserEmail
        {
            get => _newUserEmail;
            set => SetProperty(ref _newUserEmail, value);
        }

        private string _newUserPassword;
        public string NewUserPassword
        {
            get => _newUserPassword;
            set => SetProperty(ref _newUserPassword, value);
        }

        public IAsyncRelayCommand LoadUsersCommand { get; }
        public IAsyncRelayCommand AddUserCommand { get; }
        public IAsyncRelayCommand UpdateUserCommand { get; }
        public IAsyncRelayCommand DeleteUserCommand { get; }
        public IRelayCommand UnselectUserCommand { get; }

        private async Task LoadUsersAsync()
        {
            try
            {
                Users.Clear();
                var users = await _userRepository.GetAllUsersAsync();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load users");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to load users", "OK");
            }
        }

        private async Task AddUserAsync()
        {
            try
            {
                var newUser = new User
                {
                    Name = NewUserName,
                    Email = NewUserEmail,
                    Password = NewUserPassword,
                    Role = RoleEnum.Employee // Default role, you can modify this if needed
                };

                await _userRepository.AddUserAsync(newUser);
                Users.Add(newUser);

                ClearForm();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add user");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to add user", "OK");
            }
        }

        private async Task UpdateUserAsync()
        {
            try
            {
                if (SelectedUser != null)
                {
                    SelectedUser.Name = NewUserName;
                    SelectedUser.Email = NewUserEmail;

                    await _userRepository.UpdateUserAsync(SelectedUser);
                    await LoadUsersAsync(); // Reload users after update
                    UnselectUser();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to update user", "OK");
            }
        }

        private async Task DeleteUserAsync()
        {
            try
            {
                if (SelectedUser != null)
                {
                    await _userRepository.DeleteUserAsync(SelectedUser.Id.ToString());
                    Users.Remove(SelectedUser);
                    await LoadUsersAsync(); // Reload users after delete
                    UnselectUser();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete user", "OK");
            }
        }

        private void UnselectUser()
        {
            SelectedUser = null;
            ClearForm();
        }

        private void ClearForm()
        {
            NewUserName = string.Empty;
            NewUserEmail = string.Empty;
            NewUserPassword = string.Empty;
        }
    }
}
