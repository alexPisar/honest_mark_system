using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;

namespace UtilitesLibrary.ModelBase
{
    public class DataGridViewModel<TEntity> : ListViewModel<TEntity>, FilterDataGrid.Interfaces.IFilterDataGridContext where TEntity : class
    {
        private string _search = string.Empty;
        private ICollectionView _collView;
        private FilterDataGrid.FilterDataGrid _control;

        public void InitContext(FilterDataGrid.FilterDataGrid control) => _control = control;

        public ObservableCollection<TEntity> SourceItemsList { get; set; } = new ObservableCollection<TEntity>();

        /// <summary>
            ///     Global filter
            /// </summary>
        public string Search
        {
            get => _search;
            set
            {
                _search = value;

                if (_control == null)
                    return;

                _collView.Filter = e =>
                {
                    var item = e as TEntity;

                    if (item == null)
                        return false;

                    var columns = _control?.Columns;

                    if (columns == null)
                        return false;

                    bool result = false;

                    foreach (var col in columns)
                    {
                        if (result)
                            break;

                        if (col as System.Windows.Controls.DataGridTextColumn == null)
                            continue;

                        if (col as FilterDataGrid.DataGridTextColumn != null)
                            if (!((FilterDataGrid.DataGridTextColumn)col).IsColumnFiltered)
                                continue;

                        var textColumn = col as System.Windows.Controls.DataGridTextColumn;
                        var fieldName = (textColumn.Binding as Binding)?.Path?.Path;

                        if (string.IsNullOrEmpty(fieldName))
                            continue;

                        var fieldValueObj = item.GetType()?.GetProperty(fieldName)?.GetValue(item);// as string;

                        string fieldValue = null;

                        if (fieldValueObj != null)
                        {
                            if (fieldValueObj as string == null)
                                fieldValue = fieldValueObj.ToString();
                            else
                                fieldValue = fieldValueObj as string;
                        }

                        result = result || fieldValue != null && fieldValue.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    return result;
                };

                if (_collView != null)
                {
                    _collView.Refresh();
                    ItemsList = new ObservableCollection<TEntity>(_collView.OfType<TEntity>());
                }

                OnPropertyChanged(nameof(Search));
                OnPropertyChanged(nameof(ItemsList));
            }
        }
        /// <summary>
            ///     Fill data
            /// </summary>
        private void FillData()
        {
            _search = string.Empty;
            _collView = CollectionViewSource.GetDefaultView(ItemsList);

            OnPropertyChanged("Search");
            this.OnPropertyChanged("ItemsList");
            this.OnAllPropertyChanged();
        }

        public override void Refresh()
        {
            ItemsList = new ObservableCollection<TEntity>(SourceItemsList);
            Task.Run(() => FillData());
        }
    }
}
