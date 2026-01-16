interface DocumentListPaginationProps {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
}

export function DocumentListPagination({
  currentPage,
  totalPages,
  pageSize,
  totalCount,
  onPageChange,
  onPageSizeChange,
}: DocumentListPaginationProps): JSX.Element {
  const startItem = (currentPage - 1) * pageSize + 1;
  const endItem = Math.min(currentPage * pageSize, totalCount);
  
  const handlePreviousPage = () => {
    if (currentPage > 1) {
      onPageChange(currentPage - 1);
    }
  };

  const handleNextPage = () => {
    if (currentPage < totalPages) {
      onPageChange(currentPage + 1);
    }
  };

  const handlePageSizeChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    onPageSizeChange(Number(event.target.value));
  };
  
  return (
    <div className="document-list-pagination" role="navigation" aria-label="Document list pagination">
      <div className="document-list-pagination__info">
        <span>
          Showing {startItem} to {endItem} of {totalCount} documents
        </span>
        <label className="document-list-pagination__page-size">
          <span className="sr-only">Items per page</span>
          <select
            value={pageSize}
            onChange={handlePageSizeChange}
            className="document-list-pagination__select"
            aria-label="Items per page"
          >
            <option value={10}>10 per page</option>
            <option value={20}>20 per page</option>
            <option value={50}>50 per page</option>
          </select>
        </label>
      </div>
      
      <div className="document-list-pagination__controls">
        <button
          type="button"
          onClick={handlePreviousPage}
          disabled={currentPage === 1}
          className="document-list-pagination__button"
          aria-label="Previous page"
        >
          ← Previous
        </button>
        
        <span className="document-list-pagination__current" aria-current="page">
          Page {currentPage} of {totalPages}
        </span>
        
        <button
          type="button"
          onClick={handleNextPage}
          disabled={currentPage === totalPages}
          className="document-list-pagination__button"
          aria-label="Next page"
        >
          Next →
        </button>
      </div>
    </div>
  );
}
