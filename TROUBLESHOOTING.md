# Troubleshooting Guide

## Visual Effects Not Showing

### Symptoms
- Window appears fully opaque (not transparent)
- No blur or glass effect visible
- Settings toggles don't seem to have any effect

### Solutions

#### 1. Check Windows Version
- **Mica Effect**: Requires Windows 11
- **Acrylic Effect**: Requires Windows 10 version 1803 or later
- **Opacity**: Works on all Windows versions

#### 2. Check Application Logs
The application now logs detailed information about visual effects:
```
Visual effects applied - Transparency: True, Glass: True
Applied Mica effect to window: SyncFlow
```

Or if there's an issue:
```
Mica effect not supported, falling back to Acrylic
Acrylic effect not supported, using opacity only
```

#### 3. Verify Settings
1. Open Settings (⚙️ button)
2. Check "Enable Transparency" checkbox
3. Check "Enable Glass Effect" checkbox (optional)
4. Click Save
5. Restart the application

#### 4. Check System Settings
- **Windows 11**: Settings → Personalization → Colors → Transparency effects (must be ON)
- **Windows 10**: Settings → Personalization → Colors → Transparency effects (must be ON)

#### 5. Manual Test
If effects still don't work, the window should at least show opacity changes:
- With transparency enabled: Window opacity = 0.95 (slightly see-through)
- With transparency disabled: Window opacity = 1.0 (fully opaque)

## Delete Button Not Working

### Symptoms
- Clicking the delete button (🗑️) does nothing
- No confirmation dialog appears
- Profile remains in the list

### Solutions

#### 1. Check Profile State
The delete button only works when the profile is in **Idle** state:
- Status should show "Ready"
- Profile should NOT be running a transfer
- Profile should NOT be in "Completed" or "Failed" state

If the profile is in Completed or Failed state, click the "Reset" button first, then try deleting.

#### 2. Check Application Logs
The application now logs detailed information about delete operations:
```
ExecuteDelete called for profile: [name], State: Idle
Invoking DeleteRequested event for profile: [name]
Delete requested for profile: [name] (ID: [guid])
User response to delete confirmation: Yes
Deleting profile from service: [name]
Profile deleted successfully: [name]
```

If you see:
```
DeleteRequested event has no subscribers for profile: [name]
```
This indicates an event wiring issue - please report this as a bug.

#### 3. Try These Steps
1. Ensure the profile shows "Ready" status
2. Click the delete button (🗑️)
3. Wait for the confirmation dialog
4. Click "Yes" in the dialog
5. Look for the success message

#### 4. Alternative: Restart Application
If the delete button is completely unresponsive:
1. Close the application
2. Reopen it
3. Try deleting the profile again

## Multiple Folder Mappings Not Saving

### Symptoms
- Added multiple source-to-destination mappings in profile editor
- After saving, only one mapping appears
- Mappings are lost after reopening the profile

### Solutions

#### 1. Verify Mappings Before Saving
In the Profile Editor:
1. Check that all mappings are listed
2. Each mapping should show:
   - Source path
   - Destination path
   - Remove button (🗑️)
3. Click "Save" (not Cancel)

#### 2. Check for Validation Errors
If you see an error message like:
- "At least one folder mapping is required"
- "Folder mapping X: Source path is required"
- "Folder mapping X: Source folder does not exist"

Fix the validation errors before saving.

#### 3. Verify After Saving
1. Save the profile
2. Immediately edit it again
3. Check if all mappings are still there
4. If not, check application logs for errors

#### 4. Check Profile Storage
Profiles are stored in JSON format at:
```
%LOCALAPPDATA%\SyncFlow\profiles.json
```

You can open this file to verify your mappings are being saved correctly.

## General Troubleshooting

### Enable Logging
The application uses Microsoft.Extensions.Logging. To see detailed logs:

1. **During Development**: Logs appear in the Debug output window in Visual Studio
2. **In Production**: Consider adding a file logger or console logger

### Check for Exceptions
Look for error dialogs that might appear when:
- Saving profiles
- Deleting profiles
- Applying visual effects
- Loading profiles on startup

### Reset Application Data
If all else fails, you can reset the application by deleting:
```
%LOCALAPPDATA%\SyncFlow\
```

**Warning**: This will delete all your profiles!

### Report Issues
If you encounter issues not covered here, please report them with:
1. Steps to reproduce
2. Application logs (if available)
3. Windows version
4. Screenshots (if applicable)
