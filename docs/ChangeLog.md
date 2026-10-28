# Phase 2 → Phase 3A Change Log

| Date of delivered draft | Existing Phase 2 area | 3A change and reason |
|---|---|---|
| 2026-10-27 | `Models/BakeryProduct`, `Customer`, `OrderItem`, `BakeryOrder`, `PickupWindow` | Replaced empty Phase 2 stubs with serializable properties, constructors and validation so the app can store real bakery data. |
| 2026-10-27 | `Models/OrderStatus` | Replaced preview lifecycle names with assignment states: Received, Baking, Ready, PickedUp, Cancelled. |
| 2026-10-27 | `Models` | Added `BakeryCategory`, `BakerySettings`, and `VariantRules` to store menu categories, tax settings and seasonal item selection as data. |
| 2026-10-27 | `Services/CatalogService` | Added filtering and season visibility methods for reuse in Week 2. |
| 2026-10-27 | `Data` (new) | Added `SeedData`, `DataStore`, `AppData` for JSON save/reload across restarts; 16 products, three test customers and eight sample orders. |
| 2026-10-27 | `Forms/WelcomeForm` | Added the 3A demo entry point without replacing the original Phase 2 form designer. |
| 2026-10-27 | `Forms/SavedDataForm` (new) | Added testable lists, sample-order save, reload and variant setup, so 3A can be demonstrated without misrepresenting the 3B/3C work. |
| 2026-10-27 | `Program.cs` | Initialize data at application startup; show a friendly error when a JSON file cannot be read. |

These are source changes in the provided draft. Update the dated entries to reflect your **actual** work and commits before submitting.


### Navigation placement improvement (Phase 3A)
- `BakeryApplicationContext.GoTo`: copying window bounds and maximize status prevents new forms from recentering on navigation.
- `WelcomeForm`: the Phase 3A data demo now starts at the current Welcome window position.
- These are code changes, not a recorded Git commit; only log them as your own development work when tested and committed.

### Catalog filtering improvement (Phase 3A V4)
- `Forms/CustomerOrderForm.cs`: replaced four hardcoded preview products at runtime with a scrollable list driven by `CatalogService` and `products.json`. Connected `SelectedIndexChanged` so every category choice refreshes the list, including seasonal availability.
- `Tests/Program.cs`: added category and seasonal visibility checks.
- Purpose: make the ORDER filter functional without implementing 3B's cart or changing previously implemented window positioning. Record the real work date and commit when verified on Windows.

### Interface and naming cleanup
- Renamed the data viewer and its sample-order method for clarity.
- Standardized form labels and removed outdated phase markers from customer-facing screens.
- Simplified comments while keeping the Phase 2 classes and navigation structure.
- These changes still require a local Visual Studio build and a real GitHub commit before submission.

### Order menu layout correction (Phase 3A)
- `Forms/CustomerOrderForm.cs`: the scrollable product list now fills the catalog panel when resized. The product card, picture, name, description, price, and add button are resized from the current viewport dimensions instead of staying at the original fixed size.
- `Navigation/ResponsivePageLayout.cs`: after the page layout changes, refresh the catalog's inner list dimensions.
- Verify at normal and maximized window sizes in Visual Studio and record the actual date and commit after testing.
