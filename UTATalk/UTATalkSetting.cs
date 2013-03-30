using System.ComponentModel;
using FNF.XmlSerializerSetting;

namespace Plugin_UTATalk {

  public class UTATalkSettingFormData : ISettingFormData {
    private UTATalkSetting     setting;
    public  UTATalkSettingView settingView;

    public UTATalkSettingFormData(UTATalkSetting setting) {
      this.setting = setting;
      settingView = new UTATalkSettingView(setting);
    }

    public string Title {
      get { return setting.Plugin.Name; }
    }

    public bool ExpandAll {
      get { return false; }
    }

    public SettingsBase Setting {
      get { return setting; }
    }
  }

  public class UTATalkSettingView : ISettingPropertyGrid {
    private UTATalkSetting setting;

    public UTATalkSettingView(UTATalkSetting setting) {
      this.setting = setting;
    }

    public string GetName() {
      return "UTAU読み上げ設定";
    }

    [Category("基本設定")]
    [DisplayName("UTAU音源ライブラリパス")]
    [Description("oto.iniへのパスを指定してください。")]
    [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public string VoiceBankDefinitionPath {
      get { return setting.VoiceBankDefinitionPath; }
      set { setting.VoiceBankDefinitionPath = value; }
    }

    [Category("基本設定")]
    [DisplayName("resampler パス")]
    [Description("resampler へのパスを指定してください。")]
    [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public string ResamplerPath {
      get { return setting.ResamplerPath; }
      set { setting.ResamplerPath = value; }
    }

        [Category("基本設定")]
    [DisplayName("wavetool パス")]
    [Description("wavtool.exe へのパスを指定してください。")]
    [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public string WaveToolPath {
      get { return setting.WaveToolPath; }
      set { setting.WaveToolPath = value; }
    }
  }

  public class UTATalkSetting : SettingsBase {
    public string VoiceBankDefinitionPath = "";
    public string ResamplerPath           = "";
    public string WaveToolPath            = "";

    internal Plugin_UTATalk Plugin;

    public UTATalkSetting() {
    }

    public UTATalkSetting(Plugin_UTATalk utaTalk) {
      this.Plugin = utaTalk;
    }
  }
}