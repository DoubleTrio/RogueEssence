using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Avalonia.Controls;
using RogueElements;
using System.Collections;
using System.Threading.Tasks;
using Avalonia.Input;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.Views;

// public class DictionaryElement
// {
//     private object key;
//     public object Key
//     {
//         get { return key; }
//     }
//     private object value;
//     public object Value
//     {
//         get { return value; }
//     }
//     public string DisplayValue
//     {
//         get { return conv.GetString(value); }
//     }
//
//     private StringConv conv { get; }
//
//     public DictionaryElement(StringConv conv, object key, object value)
//     {
//         this.conv = conv;
//         this.key = key;
//         this.value = value;
//     }
//
// }

namespace RogueEssence.Dev.ViewModels
{
    public class DictionaryElement : ViewModelBase
    {
        private object _key;
        public object Key
        {
            get => _key;
            private set => this.RaiseAndSetIfChanged(ref _key, value);
        }

        private object _value;
        public object Value
        {
            get => _value;
            private set
            {
                Console.WriteLine(_value + "changing");
                this.RaiseAndSetIfChanged(ref _value, value);
            }
        }

        public string DisplayValue => conv.GetString(_value);

        private StringConv conv { get; }

        public DictionaryElement(StringConv conv, object key, object value)
        {
            this.conv = conv;
            _key = key;
            _value = value;

            Console.WriteLine(value.GetType() + "value");
            this.WhenAnyValue(x => x.Value)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(DisplayValue)));
        }

        public void UpdateKey(object newKey) => Key = newKey;
        public void UpdateValue(object newValue) => Value = newValue;
    }

    
    // TODO: Allow the user to edit primitive types (probably just numbers and strings) directly through the DataGrid
    // Check for the type of the object in the key and value and have some ifs...
    public class DictionaryBoxViewModel : ViewModelBase
    {
        public bool IsKeyPrimitive => Collection.Count > 0 && DataEditor.IsDataGridEditableType(Collection[0].Key);
        public bool IsValuePrimitive => Collection.Count > 0 && DataEditor.IsDataGridEditableType(Collection[0].Value);
        
        public ObservableCollection<DictionaryElement> Collection { get; }

        private int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { this.SetIfChanged(ref selectedIndex, value); }
        }

        public delegate void EditElementOp(object oldKey, object newKey, object element);
        public delegate void ElementOp(object key, object element, bool advancedEdit, EditElementOp op);

        public event ElementOp OnAddItem;
        
        public event ElementOp OnEditKey;
        public event ElementOp OnEditItem;
        public event Action OnMemberChanged;

        public StringConv StringConv;
        
        public bool ConfirmDelete;

        private IDialogService _dialogService;
        public DictionaryBoxViewModel(IDialogService dialogService, StringConv conv)
        {
            StringConv = conv;
            _dialogService = dialogService;
            Collection = new ObservableCollection<DictionaryElement>();
        }

        public T GetDict<T>() where T : IDictionary
        {
            return (T)GetDict(typeof(T));
        }

        public IDictionary GetDict(Type type)
        {
            IDictionary result = (IDictionary)Activator.CreateInstance(type);
            foreach (DictionaryElement item in Collection)
                result.Add(item.Key, item.Value);
            return result;
        }

        public void LoadFromDict(IDictionary source)
        {
            Collection.Clear();
            foreach (object obj in source.Keys)
                Collection.Add(new DictionaryElement(StringConv, obj, source[obj]));
        }



        private void editItem(object oldKey, object key, object element)
        {
            int index = GetIndexFromKey(key);
            Collection[index] = new DictionaryElement(StringConv, Collection[index].Key, element);
            SelectedIndex = index;
            OnMemberChanged?.Invoke();
        }
        
        public async Task<bool> CheckKeyDuplicateAsync(object key)
        {
            int existingIndex = GetIndexFromKey(key);
            if (existingIndex > -1)
            {
                await MessageBoxWindowView.Show(_dialogService, $"Dictionary already contains the key \"{key}\"!", "Error", MessageBoxWindowView.MessageBoxButtons.Ok);
                return true;
            }
            return false;
        }
        
        public void UpdateKey(int index, object newKey)
        {
            if (index == -1) return;
            Collection[index].UpdateKey(newKey);
        }
        
        public void UpdateValue(int index, object newValue)
        {
            if (index == -1) return;
            Collection[index].UpdateValue(newValue);
        }

        
        private async void editKey(object oldKey, object key, object element)
        {
            bool contains = await CheckKeyDuplicateAsync(key);
            if (contains) return;
            int index = GetIndexFromKey(oldKey);
            Collection[index] = new DictionaryElement(StringConv, key, element);
            SelectedIndex = index;
            OnMemberChanged?.Invoke();
        }

        
        
        
        private async void insertKey(object oldKey, object key, object element)
        {
            bool contains = await CheckKeyDuplicateAsync(key);
            if (contains) return;
            bool advancedEdit = false;
            // OnEditItem(key, element, advancedEdit, insertItem);
            Collection.Add(new DictionaryElement(StringConv, key, element));
            SelectedIndex = Collection.Count-1;
            OnMemberChanged?.Invoke();
        }

        private void insertItem(object oldKey, object key, object element)
        {
            Collection.Add(new DictionaryElement(StringConv, key, element));
            SelectedIndex = Collection.Count-1;
            OnMemberChanged?.Invoke();
        }

        public int GetIndexFromKey(object key)
        {
            int curIndex = 0;
            foreach (DictionaryElement item in Collection)
            {
                if (item.Key.Equals(key))
                    return curIndex;
                curIndex++;
            }
            return -1;
        }

        public void EditKey(int index)
        {
            bool advancedEdit = false;
            if (index > -1)
            {
                DictionaryElement item = Collection[index];
                OnEditKey?.Invoke(item.Key, item.Value, advancedEdit, editKey);
            }
        }

        public void lbxCollection_DoubleClick(object sender, DataGridCellPointerPressedEventArgs e)
        {
            //int index = lbxDictionary.IndexFromPoint(e.X, e.Y);
            int index = SelectedIndex;
            KeyModifiers modifiers = e.PointerPressedEventArgs.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            if (index > -1)
            {
                DictionaryElement item = Collection[index];
                // Console.WriteLine();
                // Console.WriteLine("Key: " + item.IsKeyPrimitive + " Value:" + item.IsValuePrimitive);
                // Console.WriteLine(item.Key.GetType() + " " + item.Value.GetType());
                OnEditItem?.Invoke(item.Key, item.Value, advancedEdit, editItem);
            }
        }

        
        public void btnAdd_Click(bool advancedEdit)
        {
            object newKey = null;
            object element = null;
            OnAddItem?.Invoke(newKey, element, advancedEdit, insertKey);
        }

        public async void btnDelete_Click()
        {
            if (SelectedIndex > -1 && SelectedIndex < Collection.Count)
            {
                if (ConfirmDelete)
                {
                    MessageBoxWindowView.MessageBoxResult result = await MessageBoxWindowView.Show(_dialogService, "Are you sure you want to delete this item:\n" + Collection[SelectedIndex].DisplayValue, "Confirm Delete",
                        MessageBoxWindowView.MessageBoxButtons.YesNo);
                    if (result == MessageBoxWindowView.MessageBoxResult.No)
                        return;
                }

                Collection.RemoveAt(SelectedIndex);
                OnMemberChanged?.Invoke();
            }
        }
    }
}
