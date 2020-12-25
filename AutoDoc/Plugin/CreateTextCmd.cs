using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace AutoDoc {
  internal sealed class CreateTextCmd {
    public const int CommandId = 0x0100;
    public static readonly Guid CommandSet = new Guid("b8e8c183-6dcd-4c00-867f-0902725db0ca");
    private AsyncPackage Package { get; set; }
    
    private CreateTextCmd(AsyncPackage package, OleMenuCommandService commandService) {
      Package = package ?? throw new ArgumentNullException(nameof(package));
      commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

      var menuCommandID = new CommandID(CommandSet, CommandId);
      var menuItem = new MenuCommand(Execute, menuCommandID);
      commandService.AddCommand(menuItem);
    }
    
    public static CreateTextCmd Instance { get; private set; }
    
    private IAsyncServiceProvider ServiceProvider {
      get { return Package; }
    }
    
    public static async Task InitializeAsync(AsyncPackage package) {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new CreateTextCmd(package, commandService);
    }
    
    private void Execute(object sender, EventArgs e) {
      ThreadHelper.ThrowIfNotOnUIThread();
      AutoDoc.CreateText();
    }
  }
}
