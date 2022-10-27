namespace PterodactylPavlovRconClient
{
    internal class ForceVerticalScrollFlowLayoutPanel : FlowLayoutPanel
    {
        public ForceVerticalScrollFlowLayoutPanel() : base()
        {
            this.AutoScroll = true;
            this.VerticalScroll.Visible = true;
            this.VerticalScroll.Enabled = true;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.Style |= 0x00200000;
                return createParams;
            }
        }
    }
}
