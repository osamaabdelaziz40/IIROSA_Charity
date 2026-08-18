### 4.16 Module: Localization Management

**Module Owner:** Super Admin  
**Purpose:** Create and manage translation files (Arabic and English)  
**Dependencies:** All UI components  

#### Use Case UC-16.1: Create Arabic Translation

| Field | Value |
|-------|-------|
| **ID** | UC-16.1 |
| **Name** | Create Arabic Translation |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Add Arabic translation for UI text in ar.json |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System displays language files:
   - ar.json (Arabic)
   - en.json (English)
3. Super Admin selects ar.json
4. System displays current Arabic translations in editable grid:
   - Key (e.g., "families.add.title")
   - Value (Arabic text)
   - Module/Section
   - Last Modified
5. Super Admin clicks "Add New Translation"
6. System displays translation creation form:
   - Translation Key (required, e.g., "buttons.submit")
   - Arabic Text (required)
   - Module (dropdown for organization)
   - Notes (optional)
7. Super Admin enters translation key
8. Super Admin enters Arabic text
9. System validates key format (dot notation)
10. System checks if key already exists
11. System adds translation to ar.json
12. System saves file
13. System logs addition in audit log
14. System displays success message
15. Translation available in UI

**Postconditions:**
- Arabic translation created
- Available in Arabic UI

---

#### Use Case UC-16.2: Create English Translation

| Field | Value |
|-------|-------|
| **ID** | UC-16.2 |
| **Name** | Create English Translation |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Add English translation for UI text in en.json |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System displays language files
3. Super Admin selects en.json
4. System displays current English translations in editable grid
5. Super Admin clicks "Add New Translation"
6. System displays translation creation form:
   - Translation Key (required)
   - English Text (required)
   - Module (dropdown)
   - Notes (optional)
7. Super Admin enters translation key
8. Super Admin enters English text
9. System validates key format
10. System checks if key already exists
11. System adds translation to en.json
12. System saves file
13. System logs addition in audit log
14. System displays success message
15. Translation available in UI

**Postconditions:**
- English translation created
- Available in English UI

---

#### Use Case UC-16.3: Update Translation

| Field | Value |
|-------|-------|
| **ID** | UC-16.3 |
| **Name** | Update Translation |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Modify existing translation in either language file |

**Preconditions:**
- Super Admin is logged in
- Translation exists

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System displays language files
3. Super Admin selects language file (ar.json or en.json)
4. System displays translations in editable grid
5. Super Admin locates translation to update
6. Super Admin edits value text directly in grid
   - OR clicks "Edit" button for inline editing
7. Super Admin modifies translation text
8. Super Admin clicks "Save"
9. System updates translation in JSON file
10. System logs change in audit log (key, old value, new value)
11. System displays success message
12. Updated translation reflected in UI

**Postconditions:**
- Translation updated
- UI reflects change

---

#### Use Case UC-16.4: Sync Translation Keys

| Field | Value |
|-------|-------|
| **ID** | UC-16.4 |
| **Name** | Sync Translation Keys |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Ensure translation keys exist in both language files |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System provides "Sync Keys" button
3. Super Admin clicks "Sync Keys"
4. System analyzes both language files:
   - ar.json keys
   - en.json keys
5. System identifies:
   - Keys only in ar.json (missing in en.json)
   - Keys only in en.json (missing in ar.json)
   - Keys in both files
6. System displays sync report:
   - Missing keys list
   - Recommendations
7. Super Admin can:
   - Add missing keys to en.json
   - Add missing keys to ar.json
   - Create placeholders for missing translations
8. Super Admin clicks "Sync"
9. System adds missing keys to both files:
   - Creates placeholder text: "[TRANSLATION NEEDED]"
10. System saves both files
11. System logs sync in audit log
12. System displays success message
13. Both files have same keys

**Postconditions:**
- Translation keys synchronized
- Both files have matching keys
- Missing translations identified

---

#### Use Case UC-16.5: Export Translation File

| Field | Value |
|-------|-------|
| **ID** | UC-16.5 |
| **Name** | Export Translation File |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Export language JSON file for external translation |

**Preconditions:**
- Super Admin is logged in
- Translation file exists

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System displays language files
3. Super Admin selects file to export (ar.json or en.json)
4. System displays "Export" button
5. Super Admin clicks "Export"
6. System displays export options:
   - Export Format (JSON, Excel, CSV)
   - Include Metadata (keys, modules, last modified)
   - Filter by Module (optional)
7. Super Admin selects export options
8. Super Admin clicks "Generate Export"
9. System generates export file
10. System downloads file to user's device
11. System logs export in audit log
12. File can be sent to external translators
13. External translators work on file without system access

**Postconditions:**
- Translation file exported
- Ready for external translation

---

#### Use Case UC-16.6: Import Translation File

| Field | Value |
|-------|-------|
| **ID** | UC-16.6 |
| **Name** | Import Translation File |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Import translated language JSON file |

**Preconditions:**
- Super Admin is logged in
- Translation file exists (from external translators)

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System displays "Import" button
3. Super Admin clicks "Import"
4. System displays import interface:
   - Select File button
   - Target Language (dropdown: ar.json, en.json)
   - Import Options:
     - Overwrite Existing (checkbox)
     - Add New Keys Only (checkbox)
     - Create Backup (checkbox, default: true)
5. Super Admin selects file
6. Super Admin selects target language
7. Super Admin selects import options
8. System validates file format (valid JSON)
9. System validates file structure (matches translation format)
10. System displays preview:
    - Keys to import
    - Keys to overwrite
    - New keys to add
    - Validation errors (if any)
11. Super Admin reviews preview
12. Super Admin confirms import
13. System creates backup of current file (if option selected)
14. System processes import:
    - Adds new keys
    - Updates existing keys (if overwrite enabled)
15. System saves updated JSON file
16. System logs import in audit log
17. System displays success message
18. New translations available in UI

**Postconditions:**
- Translation file imported
- UI updated with new translations
- Backup created (if enabled)

---

#### Use Case UC-16.7: View Missing Translations

| Field | Value |
|-------|-------|
| **ID** | UC-16.7 |
| **Name** | View Missing Translations |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Identify translation keys missing in either language |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System provides "View Missing" button
3. Super Admin clicks "View Missing"
4. System analyzes both language files
5. System displays missing translations report:
   - **Keys missing in ar.json:**
     - List of keys in en.json but not ar.json
     - English text for reference
   - **Keys missing in en.json:**
     - List of keys in ar.json but not en.json
     - Arabic text for reference
   - **Placeholder translations:**
     - Keys with placeholder text
6. System provides filter by module
7. System provides export of missing list
8. Super Admin can add missing translations directly from report
9. System shows translation completion percentage

**Postconditions:**
- Missing translations identified
- Can be addressed systematically
- Completion metric visible

---

#### Use Case UC-16.8: Set Default Language

| Field | Value |
|-------|-------|
| **ID** | UC-16.8 |
| **Name** | Set Default Language |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Configure system default language (Arabic) |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System displays "Language Settings" section
3. System shows current default language: Arabic (ar.json)
4. Super Admin can change default language dropdown:
   - Arabic (ar.json)
   - English (en.json)
5. System warns: "Changing default language affects all new users"
6. Super Admin selects default language
7. Super Admin clicks "Save Settings"
8. System updates system configuration
9. System logs change in audit log
10. System displays success message
11. New users will have selected language as default
12. Existing users keep their language preference

**Business Rules:**
- System default: Arabic (ar.json)
- Can be changed but Arabic recommended
- User preferences override system default

**Postconditions:**
- Default language configured
- Affects new users

---

#### Use Case UC-16.9: Add New Language

| Field | Value |
|-------|-------|
| **ID** | UC-16.9 |
| **Name** | Add New Language |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Add support for additional language beyond Arabic/English |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System displays "Add Language" button
3. Super Admin clicks "Add Language"
4. System displays language creation form:
   - Language Name (e.g., "French")
   - Language Code (ISO code, e.g., "fr")
   - Text Direction (dropdown: LTR, RTL)
   - Copy from Existing (dropdown: ar.json, en.json)
5. Super Admin fills in language details
6. Super Admin selects "Copy from Existing" to use as template
7. System validates language code uniqueness
8. System creates new language JSON file (e.g., fr.json)
9. System copies keys from selected template
10. System sets placeholder translations
11. System logs language creation in audit log
12. System displays success message
13. Language available for translation
14. Language switcher includes new language

**Postconditions:**
- New language added
- Ready for translation
- Available in UI

---

#### UseCase UC-16.10: Validate Translation Syntax

| Field | Value |
|-------|-------|
| **ID** | UC-16.10 |
| **Name** | Validate Translation Syntax |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Validate JSON syntax and structure of translation files |

**Preconditions:**
- Super Admin is logged in
- Translation files exist

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System provides "Validate Files" button
3. Super Admin clicks "Validate Files"
4. System validates both language files:
   - ar.json syntax
   - en.json syntax
5. System checks for:
   - Valid JSON format
   - Duplicate keys
   - Invalid characters
   - Malformed keys
   - Empty values (warnings)
   - Key format compliance (dot notation)
6. System displays validation report:
   - Validation Status (Passed/Failed)
   - Errors found (if any)
   - Warnings found (if any)
   - File size
   - Key count
7. If errors found:
   - System shows error details
   - System shows line numbers
   - System suggests fixes
8. If validation passed:
   - System displays "All files valid"
9. System logs validation in audit log
10. Super Admin can fix errors and re-validate

**Postconditions:**
- Translation files validated
- Errors identified
- Syntax confirmed correct

---


