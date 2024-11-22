using Launcher.Views;

namespace Launcher;

public class DesignDialog : DialogBase
{
    public DesignDialog()
    {
        PageName = "Design Dialog";
    }
}

public static class DesignData
{
    public static DesignDialog DesignDialogBase = new DesignDialog()
    {
        // PageName = "Design Dialog Base"
    };
}