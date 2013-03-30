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
      return "UTAU�ǂݏグ�ݒ�";
    }

    [Category("��{�ݒ�")]
    [DisplayName("UTAU�������C�u�����p�X")]
    [Description("oto.ini�ւ̃p�X���w�肵�Ă��������B")]
    [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public string VoiceBankDefinitionPath {
      get { return setting.VoiceBankDefinitionPath; }
      set { setting.VoiceBankDefinitionPath = value; }
    }

    [Category("��{�ݒ�")]
    [DisplayName("resampler �p�X")]
    [Description("resampler �ւ̃p�X���w�肵�Ă��������B")]
    [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public string ResamplerPath {
      get { return setting.ResamplerPath; }
      set { setting.ResamplerPath = value; }
    }
  }

  public class UTATalkSetting : SettingsBase {
    public string VoiceBankDefinitionPath = "";
    public string ResamplerPath           = "";

    internal Plugin_UTATalk Plugin;

    public UTATalkSetting() {
    }

    public UTATalkSetting(Plugin_UTATalk utaTalk) {
      this.Plugin = utaTalk;
    }
  }
}