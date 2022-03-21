namespace Pori.Frends.Data
{
    /// <summary>
    /// The type of input to provide a function processing a table's rows.
    /// </summary>
    public enum ProcessingType
    {
        /// <summary>
        /// Process rows using the entire row.
        /// </summary>
        Row,

        /// <summary>
        /// Process rows using the value of single column.
        /// </summary>
        Column
    }

    /// <summary>
    /// Whether to sort table rows in an ascending or descending order.
    /// </summary>
    public enum Order
    {
        /// <summary>
        /// Sort table rows in ascending order.
        /// </summary>
        Ascending,

        /// <summary>
        /// Sort table rows in descending order.
        /// </summary>
        Descending
    }
}