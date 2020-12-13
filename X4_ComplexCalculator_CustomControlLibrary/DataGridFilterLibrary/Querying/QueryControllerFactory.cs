using System.Collections;
using X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Support;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Querying
{
    public class QueryControllerFactory
    {
        public static QueryController 
            GetQueryController(
            System.Windows.Controls.DataGrid dataGrid,
            FilterData filterData, IEnumerable itemsSource)
        {
            QueryController? query;

            query = DataGridExtensions.GetDataGridFilterQueryController(dataGrid);

            if (query is null)
            {
                //clear the filter if exisits begin
                System.ComponentModel.ICollectionView view
                    = System.Windows.Data.CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
                if (view != null) view.Filter = null;
                //clear the filter if exisits end

                query = new QueryController(filterData, itemsSource, dataGrid.Dispatcher, DataGridExtensions.GetUseBackgroundWorkerForFiltering(dataGrid));
                DataGridExtensions.SetDataGridFilterQueryController(dataGrid, query);

                return query;
            }

            query.ColumnFilterData        = filterData;
            query.ItemsSource             = itemsSource;
            query.CallingThreadDispatcher = dataGrid.Dispatcher;
            query.UseBackgroundWorker     = DataGridExtensions.GetUseBackgroundWorkerForFiltering(dataGrid);

            return query;
        }
    }
}
