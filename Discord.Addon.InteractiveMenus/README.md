# Disord.Addon.InteractiveMenus
An API and Service for creating and managing interactive menus that use reactions as input from the user.

## Usage
In order to create a menu, first you must create a new instance of the type of menu you want to create.
Let's try creating a Selector Menu to let the user pick between a list of objects.
```csharp
using Discord.Addons.InteractiveMenus;

myclass[] My_objects; // This is the list of objects we want the user to choose from.
string[] My_Names = My_Objects.Select(x=>x.Name); // This is the list of the names of the objects in the same order as My_Objects.

// We First crate our instance of the Selector Menu
SelectorMenu Menu = New SelectorMenu("Please select from one of these options.",My_Names,My_Objects);

// Then we call the service to start this menu. We use the last bool to set if we want 
// *only* the user who invoked the menu to be able to iteract with it.
await MenuService.CreateMenu(SocketCommandContext, Menu, true);

// Now we await this function to freeze command execution until an option is picked by the user.
MyClass Result = await Menu.GetObject(); 
```