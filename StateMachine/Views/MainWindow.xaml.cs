using System;
using System.IO;
using System.Windows;
using StateMachine.Animation;
using StateMachine.ViewModels;

namespace StateMachine.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                
                // Update spritesheet path handling
                string spritesheetPath = SpritesheetHelper.GetSpritesheetPath();
                Console.WriteLine($"Looking for spritesheet at: {spritesheetPath}");
                
                if (!File.Exists(spritesheetPath))
                {
                    MessageBox.Show(
                        $"Spritesheet not found at {spritesheetPath}. " +
                        $"Please copy the Mario spritesheet to this location and restart the application.",
                        "Spritesheet Not Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
                
                try
                {
                    DataContext = new StateMachineViewModel();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error initializing ViewModel: {ex.Message}\n{ex.StackTrace}",
                        "ViewModel Initialization Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error during MainWindow initialization: {ex.Message}\n{ex.StackTrace}",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
