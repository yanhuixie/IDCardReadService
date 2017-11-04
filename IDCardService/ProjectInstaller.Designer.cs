namespace IDCardService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.IDCardServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.IDCardServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // IDCardServiceProcessInstaller
            // 
            this.IDCardServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.IDCardServiceProcessInstaller.Password = null;
            this.IDCardServiceProcessInstaller.Username = null;
            // 
            // IDCardServiceInstaller
            // 
            this.IDCardServiceInstaller.Description = "从二代身份证读卡器读取身份证信息";
            this.IDCardServiceInstaller.DisplayName = "二代身份证读卡服务";
            this.IDCardServiceInstaller.ServiceName = "IDCardService";
            this.IDCardServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.IDCardServiceProcessInstaller,
            this.IDCardServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller IDCardServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller IDCardServiceInstaller;
    }
}