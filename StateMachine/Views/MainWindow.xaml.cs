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
            
            DataContext = new StateMachineViewModel();
        }
    }
}
