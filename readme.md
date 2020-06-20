# X4: Foundations Complex calclator

# Summary
This application is station calclator for X4: Foundations.

## Main features
1. Direct data extraction from game files.
    1. Support mods.
    1. Support for future versions.
1. Calculation of the ware required for construction, taking into account the armament
1. Storage allocation capacity calculation
1. Easy Localization.


## Tested environment
- .NET Core 3.1 runtime(**REQUIRED**)
    - You can download runtime [here](https://dotnet.microsoft.com/download/dotnet-core/current/runtime)
- Windows10 64bit Version 1903


## How to start
1. Launch the X4_ComplexCalculator.
1. Extracting data from a game file. (Only at first startup)
    1. Select the folder to install X4 in the "X4:Foundations root directory".
    1. Select a language.
    1. Click the "Export" button.
    1. When the extraction is finished, press the Close button to close the dialog.

## Detailed description.
1. Main window
    1. Menu
        1. File(F)
            1. New ----- Create a new plan.
            1. Save ----- Save over the current plan.
            1. Save as -- Save your current plan under a different name.
            1. Open ---- Open the saved plan.
            1. Import
                1. Station Calclator ------ Importing plan from [Station Calclator](http://www.x4-game.com/#/station-calculator).
                1. Existing plan ---------- Import plans saved during the game.
                1. Module equipment --- Imports equipment presets for modules saved during the game.
            1. Export
                1. Station Calclator --- Exporting the plan to [Station Calclator](http://www.x4-game.com/#/station-calculator).
        1. Layout(L)
            1. Save layout --- Save the screen layout of the selected plan.
            1. Layout list ---- You can select a saved layout.
                1. Click on the "💾" button to save the layout overwritten.
                1. Click on the "✏" button to edit the layout name.
                1. Click on the "🗑" button to delete the layout.

    1. Plan area
        - Items in the plan can be hidden by clicking the "X" button.
            - The hidden items can be redisplayed by clicking [View (V)] at the top of the work area.
        1. Modules tab
            - You can narrow down the displayed modules by typing a string in the text box labeled "Search by entering text".
            - Click the "Add" button to display the module selection screen.
            - Click the "Merge" button to the same module is combined into one record.
            - Click the "Auto add" button to A module will be added to produce the missing product.
            - Clickable cells are underlined in the string.
                - You can change the module by clicking on the module name cell.
                -  You can change the number of modules by clicking the cell of the number of modules.
                - Clicking on the "Edit" cell of a module with a equipment will launch the armament editing screen.
                - You can select a construction method from the list by clicking the "Build method" cell of a module with multiple construction methods.
                - When you hover over the number of turrets and shields in a module with equipment, you will see the current equipment.
                - After selecting a module, you can copy, paste and delete it by right-clicking on it.
            - Sub-windows of the modules tab
                1. Select module window
                    - You can narrow down the displayed modules by checking or unchecking the list of "Module type"/"Owner faction", or by entering a string in the text box that says "Search by entering text".
                    - The selected module is added to the module list of the main screen when the module to be added is selected from the module list and the "Select" button is pushed.
                1. Edit equipment window
                    - You can narrow down the equipment list by selecting the "Owner faction" list.
                    - You can select the size of the equipment (medium/large etc.) in the "Equipment Size" combo box.
                    - Select the items you want to equip in the list of equipment and press the "→" button to equip the selected items in the module.
                        - You can add a selection at once by clicking while holding down the left shift.
                    - Click the "+" button to create a preset based on the current equipment.
                    - Click the "🗑" button to delete the preset.
                    - Click the "✏" button to you can edit the name of the preset.
                    - Click the "💾" button to save the preset overwritten.
        1. Summary tab
            - Click the "Ⓥ" button to expand the item.
            1. Work force
                1. Necessary ware info
                    - The number of ware required for the current capacity is displayed for each race.
                    - If you click on a table heading, it will be sorted by the item you clicked on.
                1. Module info
                    - View a list of modules that require or provide the number of workers in your current plan.
                    - If you click on a table heading, it will be sorted by the item you clicked on.
            1. Profit per hour
                - It shows the profit and loss per hour for each product.
                - If you click on a table heading, it will be sorted by the item you clicked on.
            1. Build cost
                - A list of building costs for the current plan is displayed.
                - If you click on a table heading, it will be sorted by the item you clicked on.
        1. Products tab
            - Click the "+" button on the left to see the modules related to the product.
            - You can show/hide the modules related to the selected product from the right-click menu at once.
            - The background color of the cell turns red when there is a shortage of product.
            - Click the "Unit Price" cell to set the unit price of the product.
                - The same operation can be performed by manipulating the slider in the right column of the "Unit Price" cell.
                - By manipulating the slider in the heading section of the slider column, you can change the amount of money for all products at once.
        1. Build resources tab
            - Click the "Unit Price" cell to set the unit price of the product.
                - The same operation can be performed by manipulating the slider in the right column of the "Unit Price" cell.
                - By manipulating the slider in the heading section of the slider column, you can change the amount of money for all products at once.
        1. Storages tab
            - Clicking the "+" button on the left side of the screen will display the modules for each type of vault.
        1. Storage assign tab
            - Operate the slider at the top to calculate the status of the storage after n hours.
                - The maximum value of the slider can be changed in the text box to the right of the slider.
            - The ware is grouped and displayed by storage type.
                - Click the "Ⓥ" button to the left of the vault type to collapse/expand the group.
            - The "Allocated" cell can be increased only by the value of the "Allocable" cell.
            - If the number of cells after the specified time is negative, the background color of the cell turns red.
            - When "Allocated" < "Number after the specified time", the background color of the cell will be yellow.

## Tips
- Plan or items in the plan area (such as modules tab, summary tab, etc) can be freely docked/undocked.

## How to Uninstall
The registry is not edited at all, so please delete the entire folder.

## Licenses
X4 Complex Calclator is licensed under the [Apache License 2.0](https://github.com/Ocelot1210/X4_ComplexCalculator/blob/master/LICENSE). Contributions are welcomed!