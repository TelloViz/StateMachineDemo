using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StateMachine;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // We need to have the spritesheet available for the application
        // Here we would typically extract the spritesheet from resources
        // For this example, you'll need to manually copy the spritesheet to the output directory
        
        // Display loading information and location where spritesheet should be placed
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