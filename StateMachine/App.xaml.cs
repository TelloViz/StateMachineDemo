using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace StateMachine
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Add global exception handler to catch unhandled exceptions
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}\n{e.Exception.StackTrace}",
                "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            e.Handled = true; // Prevent application from crashing
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            MessageBox.Show($"A fatal error occurred: {exception?.Message}\n{exception?.StackTrace}",
                "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

