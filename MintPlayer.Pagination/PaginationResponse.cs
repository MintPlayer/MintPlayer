using System.Collections.Generic;
using System.Linq;

namespace MintPlayer.Pagination
{
    public class PaginationResponse<TDto>
    {
        private PaginationResponse()
        {
        }
        public PaginationResponse(PaginationRequest<TDto> request, int totalRecords, IEnumerable<TDto> data) : this()
        {
            this.data = data.ToList();
            page = request.Page;
            perPage = request.PerPage;
            this.totalRecords = totalRecords;
        }

        #region Data
        private List<TDto> data;
        public List<TDto> Data
        {
            get => data;
            set { }
        }
        #endregion
        #region Page
        private int page;
        /// <summary>Current page to load, readonly.</summary>
        public int Page
        {
            get => page;
            set { }
        }
        #endregion
        #region PerPage
        private int perPage;
        /// <summary>Number of items per page, readonly.</summary>
        public int PerPage
        {
            get => perPage;
            set { }
        }
        #endregion
        #region TotalRecords
        private int totalRecords;
        /// <summary>Total number of records, readonly.</summary>
        public int TotalRecords
        {
            get => totalRecords;
            set { }
        }
        #endregion
        #region TotalPages
        /// <summary>Total number of pages, readonly.</summary>
        public int TotalPages
        {
            get => (totalRecords - 1) / perPage + 1;
            set { }
        }
        #endregion
    }
}
