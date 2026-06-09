using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using Avalonia.VisualTree;
using Avalonia.Input;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views
{
    public partial class DictionaryBox : UserControl
    {
        public DictionaryBox()
        { 
            this.InitializeComponent(); 
            DictionaryBoxAddButton.AddHandler(PointerReleasedEvent, DictionaryBoxAddButton_OnPointerReleased, RoutingStrategies.Tunnel);
        }

      
        

        public void SetListContextMenu(ContextMenu menu)
        {
            gridItems.ContextMenu = menu;
        }

        private void DictionaryBoxAddButton_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            DictionaryBoxViewModel vm = (DictionaryBoxViewModel) DataContext;
            vm.btnAdd_Click(advancedEdit);
        }
        
        private void DictionaryBoxDataGrid_OnCellPointerPressed(object sender, DataGridCellPointerPressedEventArgs e)
        {
            if (e.PointerPressedEventArgs.ClickCount != 2) return;

            ViewModels.DictionaryBoxViewModel viewModel = (ViewModels.DictionaryBoxViewModel)DataContext;
            if (viewModel == null)
                return;

            
            DictionaryElement currentItem = (DictionaryElement)e.Row.DataContext;
            if (currentItem == null) return;


            Console.WriteLine(e.Column.DisplayIndex + " " + e.Column.Header);
            Console.WriteLine(currentItem.Key.GetType() + " " + currentItem.Value.GetType());
            
            // TODO: Handle the case when key isn't a primitive type...
            if (e.Column.DisplayIndex == 0 && !viewModel.IsKeyPrimitive)
            {
                // Handle here...
                viewModel.lbxCollection_DoubleClick(sender, e);
                e.PointerPressedEventArgs.Handled = true;
            }
            else if (e.Column.DisplayIndex == 1 && !viewModel.IsValuePrimitive)
            {
                viewModel.lbxCollection_DoubleClick(sender, e);
                e.PointerPressedEventArgs.Handled = true;
            }


            Console.WriteLine(e.PointerPressedEventArgs.Handled + "handled");
        }

        private bool _isCellEditInProgress = false;

        private async void DictionaryBoxDataGrid_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Console.WriteLine(e.EditAction + " " + e.Column.DisplayIndex);
            
            if (e.EditAction != DataGridEditAction.Commit) return;
            
            if (_isCellEditInProgress) return;
            
            _isCellEditInProgress = true;
        
            
            ViewModels.DictionaryBoxViewModel vm = (ViewModels.DictionaryBoxViewModel)DataContext;
            if (vm == null) return;

            TextBox textBox = e.EditingElement as TextBox;
            if (textBox == null) return;
                
            string newText = textBox.Text;

            if (_originalCellValue == newText) return;
          
            Console.WriteLine("orig cell value " + _originalCellValue + " new cell value"  + newText);
            
            if (e.Column.DisplayIndex == 0)
            {
                
                bool isDuplicate = await vm.CheckKeyDuplicateAsync(newText);
                if (!isDuplicate)
                {
                    // vm.UpdateKey(e.Row.Index, newText);
                }
                else
                {
                    textBox.Text = _originalCellValue;
                }
            } else if (e.Column.DisplayIndex == 1)
            {
                // vm.UpdateValue(e.Row.Index, newText);
            }
            _isCellEditInProgress = false;
            
        }
        // private void PriorityListBoxDataGrid_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        // {
        //     if (_isEditingPriority) return;
        //     if (e.EditAction != DataGridEditAction.Commit) return;
        //     if (e.Column.DisplayIndex != 0) return;
        //     
        //     var textBox = e.EditingElement as TextBox;
        //     if (textBox == null) return;
        //
        //     Priority? newPriority = ParsePriority(textBox.Text);
        //     if (newPriority == null) return;
        //
        //     e.Cancel = true;
        //
        //     _isEditingPriority = true;
        //     PriorityListBoxViewModel viewModel = (PriorityListBoxViewModel)DataContext;
        //     viewModel?.ChangePriority(newPriority.Value);
        //     _isEditingPriority = false;
        // }

        private string _originalCellValue;

        private void DictionaryBoxDataGrid_OnPreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.DisplayIndex == 0)
            {
                TextBox textBox = e.EditingElement as TextBox;
                if (textBox == null) return;
                _originalCellValue = textBox.Text;
            }
        }
    }
}
