using System;
using MobaGo.UI;
using UnityEngine;

namespace MUD
{
    class UIPanel:UGUIComponent
    {

        #region Panel 静态常量
        private static string CurrentPanelPath = MPanelPathDefine.TakePicturePanel_Path;
        private static int ButtonEventID = (int)eUIEventID.Achievement_Open_Type_Form;
        #endregion

        #region UI对象路径
        private Transform Root;
        #endregion

        #region UI对象声明
        #endregion

        #region 其它变量声明
        #endregion

        /// <summary>
        /// UI对象初始化
        /// </summary>
        private void Init()
        {
            Root = this.transform;
        }

        #region Panel周期函数
        public static UIPanel OpenForm()
        {
            UIPanel panel = UGUILogicController.GetInstance().OpenForm<UIPanel>(CurrentPanelPath);
            return panel;
        }

        public override void Initialize(UGUIFormScript formScript)
        {
            if (m_isInitialized == true)
                return;
            base.Initialize(formScript);

            Init();
            RegisterEvents();
        }

        public override void Appear()
        {
            Refresh();
            base.Appear();
        }

        public override void Close()
        {
            UnregisterEvents();
            base.Close();
        }

        public override void Hide()
        {
            base.Hide();
        }

        private void Refresh()
        {
        }
        #endregion

        #region 注册事件
        private void RegisterEvents()
        {
        }

        private void UnregisterEvents()
        {
        }
        #endregion
    }
}