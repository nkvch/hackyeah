# UI of listings and lists views

Subpages presenting lists and lists of e.g. cases, messages, reports, events and messages should be based on interactive, configurable tables that ensure effective management of a large number of records by intuitively searching, sorting, filtering and exporting data to various formats

## Key features of UI lists and tables:

- a visible search box above the table, allowing you to quickly filter records after any string (e.g. case title, ID, user name),
- ability to sort each column by clicking the header (arrows ascending/descending; double click resets to default order),
- Advanced filtering:
  - context filters in headers (e.g. parameter selection from the drop-down list, date range, statuses, document types),
  - the ability to apply multiple filters at once and reset them quickly,
- pagination for large tables, with the ability to select the number of records on the page and go to the selected page number.

## Data export functions:

- a dedicated 'Export' button above or below the table, which expands the format selection menu: XLSX, CSV, JSON,
- export with current filters, sorting and selected columns,
- Information about the limit of exported records (if any),
- a feedback message after a successful or unsuccessful export attempt.

## Usable functions:

- each row contains contextual actions (e.g. 'Preview', 'Edit', 'Download', 'Delete') in the form of hamburger icons or menus,
- table headers remain glued when scrolling (sticky header),
- each of the functionalities should be clearly marked, easily accessible and react immediately (dynamic narrowing of the displayed list of results, loading animations, clear buttons).
