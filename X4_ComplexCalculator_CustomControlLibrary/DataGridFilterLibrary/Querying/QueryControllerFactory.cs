using System.Collections;
using System.Windows.Controls;
using System.Windows.Data;
using X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Support;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Querying
{
    public class QueryControllerFactory
    {
        public static QueryController
            GetQueryController(
            DataGrid dataGrid,
            FilterData filterData, IEnumerable itemsSource)
        {
            QueryController? query;

            query = DataGridExtensions.GetDataGridFilterQueryController(dataGrid);

            if (query is null)
            {
                //clear the filter if exisits begin
                System.ComponentModel.ICollectionView view
                    = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
                if (view is not null)
                {
                    view.Filter = null;
                }
                //clear the filter if exisits end

                query = new QueryController();
                DataGridExtensions.SetDataGridFilterQueryController(dataGrid, query);
            }

            query.ColumnFilterData        = filterData;
            query.ItemsSource             = itemsSource;
            query.CallingThreadDispatcher = dataGrid.Dispatcher;
            query.UseBackgroundWorker     = DataGridExtensions.GetUseBackgroundWorkerForFiltering(dataGrid);

            return query;
        }
    }
}
