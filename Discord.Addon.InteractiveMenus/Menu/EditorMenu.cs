using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace Discord.Addon.InteractiveMenus
{
	/// <summary>
	/// <para>Advanced menu that allows you to edit a file by passing a list of options with custom logic tied to them.</para>
	/// </summary>
	public class EditorMenu : Menu
	{
		private string Title;
		public int Cursor = 0;
		private object StoredObject;
		public List<EditorOption> Options;
		private bool Active = true;
		public async override Task<bool> HandleButtonPress(SocketReaction reaction)
		{
			if (!Buttons.TryGetValue(reaction.Emote, out Func<Task<bool>> Logic))
			{
				return false;
			}
			return await Logic();
		}
		/// <summary>
		/// Advanced menu that lets you modify an object with user input.
		/// </summary>
		/// <param name="Editor_Title">The title of the embed.</param>
		/// <param name="Objec_To_Edit">The object being edited.</param>
		/// <param name="Editor_Options">The list of options which contain the logic of the editor.</param>
		public EditorMenu(string Editor_Title, object Objec_To_Edit, EditorOption[] Editor_Options)
		{
			Title = Editor_Title;
			StoredObject = Objec_To_Edit;
			Options = Editor_Options.ToList();

			
		}
		public override async Task<RestUserMessage> Initialize(SocketCommandContext commandContext,MenuService menuService)
		{
			Message = await base.Initialize(commandContext, menuService);
			Buttons.Add(new Emoji("⏫"), PreviousOptionAsync);
			Buttons.Add(new Emoji("⏏"), SelectAsync);
			Buttons.Add(new Emoji("⏬"), NextOptionAsync);

			await Message.AddReactionsAsync(Buttons.Select(x => x.Key).ToArray());
			Options.Add(new EditorOption("Save Changes", "Save all changes made.", null));
			Options.Add(new EditorOption("Discard Changes", "Discard all changes made.", null));
			await ReloadMenu();
			return Message;
		}
		/// <summary>
		/// Returns the Object being edited or Null if changes are discarded.
		/// </summary>
		/// <returns>Object being edited or Null</returns>
		public async Task<object> GetObject()
		{
			while (Active) await Task.Delay(1000);
			await Message.DeleteAsync();
			return StoredObject;
		}
		private async Task<bool> NextOptionAsync()
		{
			if (Cursor + 1 >= Options.Count) Cursor = 0;
			else Cursor++;
			await ReloadMenu();
			return false;
		}

		private async Task<bool> SelectAsync()
		{
			switch (Options[Cursor].Name)
			{
				case "Save Changes":
					Active = false;
					return true;
				case "Discard Changes":
					StoredObject = null;
					Active = false;
					return true;
				default:
					var context = new OptionContext(Context, Service, ref StoredObject,Cursor,ref Options);
					StoredObject = await Options[Cursor].Logic(context);
					await ReloadMenu();
					break;
			}
			return false;
		}
		private async Task<bool> PreviousOptionAsync()
		{
			if (Cursor - 1 < 0) Cursor = Options.Count - 1;
			else Cursor--;
			await ReloadMenu();
			return false;
		}
		private async Task ReloadMenu()
		{
			var eb = new EmbedBuilder()
				.WithTitle(Title)
				.WithFooter("Use ⏫/⏬ to move the cursor and ⏏ to select.");
			for(int i = 0;i<Options.Count; i++)
			{
				eb.AddField(((Cursor==i)? "🔹 " : "")+Options[i].Name, Options[i].Description);
			}
			await Message.ModifyAsync(x => x.Content = ".");
			await Message.ModifyAsync(x => x.Embed = eb.Build());
		}
		public class EditorOption
		{
			/// <summary>
			/// This logic will be fired whenever the user selects this option on the Editor Menu.
			/// <para>It's In paramater is a class containing all the relevant info to the execution of this logic.</para>
			/// <para>All options should return the modified object.</para>
			/// </summary>
			public Func<OptionContext, Task<object>> Logic { get; private set; }
			public string Name;
			public string Description;
			/// <summary>
			/// An option for the Editor menu that is displayed a field in the Editor embed.
			/// </summary>
			/// <param name="Option_Name">The title of the option for the embed.</param>
			/// <param name="Option_Description">The contents of the embedded option.</param>
			/// <param name="Option_Logic">The logic that happens when this option is selected.</param>
			public EditorOption(string Option_Name, string Option_Description,Func<OptionContext,Task<object>>Option_Logic)
			{
				Logic = Option_Logic;
				Name = Option_Name;
				Description = Option_Description;
			}
		}
		/// <summary>
		/// The context surrounding the triggered Editor Menu Option.
		/// </summary>
		public class OptionContext
		{
			/// <summary>
			/// The Command Context of the message that triggered the menu.
			/// </summary>
			public SocketCommandContext CommandContext { get; private set; }
			/// <summary>
			/// The Menu service.
			/// </summary>
			public MenuService MenuService { get; private set; }
			/// <summary>
			/// The object being edited.
			/// </summary>
			public object EditableObject { get; set; }
			/// <summary>
			/// The current Cursor Index of the editor.
			/// </summary>
			public int CurrentIndex { get; private set; }
			/// <summary>
			/// The currently selected option. Changes done here are affect the menu.
			/// </summary>
			public EditorOption CurrentOption { get; set; }
			public OptionContext(SocketCommandContext commandContext, MenuService service, ref object editable, int Index,ref List<EditorOption> option)
			{
				CommandContext = commandContext;
				MenuService = service;
				EditableObject = editable;
				CurrentIndex = Index;
				CurrentOption = option[Index];
			}
		}
	}
}
