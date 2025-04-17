using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
                    var viewModel = new StateMachineViewModel();
                    DataContext = viewModel;
                    
                    // Provide path references to ViewModel with arrowheads
                    viewModel.RegisterTransitionPaths(
                        IdleToWalking, IdleToWalking_Forward, IdleToWalking_Backward,
                        WalkingToRunning, WalkingToRunning_Forward, WalkingToRunning_Backward,
                        IdleToRunning, IdleToRunning_Forward, IdleToRunning_Backward,
                        IdleToLookUp, IdleToLookUp_Forward, IdleToLookUp_Backward,
                        IdleToDucking, IdleToDucking_Forward, IdleToDucking_Backward
                    );
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
        
        private void OnStateBoxClicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string stateName && DataContext is StateMachineViewModel viewModel)
            {
                viewModel.ChangeStateCommand.Execute(stateName);
            }
        }
    }
}
