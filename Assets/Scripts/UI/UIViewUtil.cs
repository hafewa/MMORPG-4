using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;

/// <summary>
/// 窗口UI管理器
/// </summary>
public class UIViewUtil : Singleton<UIViewUtil>
{
    private Dictionary<string, UIWindowViewBase> m_DicWindow = new Dictionary<string, UIWindowViewBase>();

    /// <summary>
    /// 已经打开的窗口数量
    /// </summary>
    public int OpenWindowCount
    {
        get
        {
            return m_DicWindow.Count;
        }
    }

    /// <summary>
    /// 关闭所有窗口
    /// </summary>
    public void CloseAllWindow()
    {
        if (m_DicWindow != null)
        {
            m_DicWindow.Clear();
        }
    }

    //public void LoadWindowForLua(string viewName, XLuaCustomExport.OnCreate OnCreate = null, string path = null)
    //{
    //    LoadWindow(viewName, null, null, OnCreate, path);
    //}

    //public void LoadWindow(string viewName, Action<GameObject> onComplete, Action OnShow = null, XLuaCustomExport.OnCreate OnCreate = null, string path = null)
    public void LoadWindow(string viewName, Action<GameObject> onComplete, Action OnShow = null, string path = null)
    {
        if (!m_DicWindow.ContainsKey(viewName) || m_DicWindow[viewName] == null)
        {
            string newPath = string.Empty;

            if (string.IsNullOrEmpty(path))
            {
                newPath = string.Format("Download/Prefab/UIPrefab/UIWindows/pan_{0}.assetbundle", viewName);
            }
            else
            {
                newPath = path;
            }

            AssetBundleMgr.Instance.LoadOrDownload(newPath, string.Format("pan_{0}", viewName),
                (GameObject obj) =>
                {
                    obj = UnityEngine.Object.Instantiate(obj);

                    UIWindowViewBase windowBase = obj.GetComponent<UIWindowViewBase>();
                    if (windowBase == null) return;

                    if (OnShow != null)
                    {
                        windowBase.OnShow = OnShow;
                    }

                    m_DicWindow[viewName] = windowBase;

                    windowBase.ViewName = viewName;
                    Transform transParent = null;

                    switch (windowBase.containerType)
                    {
                        case WindowUIContainerType.Center:
                            transParent = UISceneCtrl.Instance.CurrentUIScene.Container_Center;
                            break;
                    }

                    obj.transform.parent = transParent;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localScale = Vector3.one;
                    obj.gameObject.SetActive(false);

                    //层级管理
                    LayerUIMgr.Instance.SetLayer(obj);

                    StartShowWindow(windowBase, true);

                    if (onComplete != null)
                    {
                        onComplete(obj);
                    }

                    //if (OnCreate != null)
                    //{
                    //    OnCreate(obj);
                    //}
                });
        }
        else
        {
            if (onComplete != null)
            {
                GameObject obj = m_DicWindow[viewName].gameObject;
                //层级管理
                LayerUIMgr.Instance.SetLayer(obj);

                onComplete(obj);
            }
        }
    }

    #region CloseWindow 关闭窗口
    /// <summary>
    /// 关闭窗口
    /// </summary>
    /// <param name="type"></param>
    public void CloseWindow(string viewName)
    {
        if (m_DicWindow.ContainsKey(viewName))
        {
            StartShowWindow(m_DicWindow[viewName], false);
        }
    }
    #endregion

    #region StartShowWindow 开始打开窗口
    /// <summary>
    /// 开始打开窗口
    /// </summary>
    /// <param name="windowBase"></param>
    /// <param name="isOpen">是否打开</param>
    private void StartShowWindow(UIWindowViewBase windowBase, bool isOpen)
    {
        switch (windowBase.showStyle)
        {
            case WindowShowStyle.Normal:
                ShowNormal(windowBase, isOpen);
                break;
            case WindowShowStyle.CenterToBig:
                ShowCenterToBig(windowBase, isOpen);
                break;
            case WindowShowStyle.FromTop:
                ShowFromDir(windowBase, 0, isOpen);
                break;
            case WindowShowStyle.FromDown:
                ShowFromDir(windowBase, 1, isOpen);
                break;
            case WindowShowStyle.FromLeft:
                ShowFromDir(windowBase, 2, isOpen);
                break;
            case WindowShowStyle.FromRight:
                ShowFromDir(windowBase, 3, isOpen);
                break;
        }
    }
    #endregion

    #region 各种打开效果

    /// <summary>
    /// 正常打开
    /// </summary>
    /// <param name="windowBase"></param>
    /// <param name="isOpen"></param>
    private void ShowNormal(UIWindowViewBase windowBase, bool isOpen)
    {
        if (isOpen)
        {
            windowBase.gameObject.SetActive(true);
        }
        else
        {
            DestroyWindow(windowBase);
        }
    }

    /// <summary>
    /// 中间变大
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="isOpen"></param>
    private void ShowCenterToBig(UIWindowViewBase windowBase, bool isOpen)
    {
        windowBase.gameObject.SetActive(true);
        windowBase.transform.localScale = Vector3.zero;
        windowBase.transform.DOScale(Vector3.one, windowBase.duration)
            .SetAutoKill(false)
            .SetEase(GlobalInit.Instance.UIAnimationCurve)
            .Pause().OnRewind(() =>
        {
            DestroyWindow(windowBase);
        });

        if (isOpen)
            windowBase.transform.DOPlayForward();
        else
            windowBase.transform.DOPlayBackwards();
    }

    /// <summary>
    /// 从不同的方向加载
    /// </summary>
    /// <param name="windowBase"></param>
    /// <param name="dirType">0=从上 1=从下 2=从左 3=从右</param>
    /// <param name="isOpen"></param>
    private void ShowFromDir(UIWindowViewBase windowBase, int dirType, bool isOpen)
    {
        windowBase.gameObject.SetActive(true);
        Vector3 from = Vector3.zero;
        switch (dirType)
        {
            case 0:
                from = new Vector3(0, 1000, 0);
                break;
            case 1:
                from = new Vector3(0, -1000, 0);
                break;
            case 2:
                from = new Vector3(-1400, 0, 0);
                break;
            case 3:
                from = new Vector3(1400, 0, 0);
                break;
        }
        windowBase.transform.localPosition = from;

        windowBase.transform.DOLocalMove(Vector3.zero, windowBase.duration)
            .SetAutoKill(false)
            .SetEase(GlobalInit.Instance.UIAnimationCurve)
            .Pause().OnRewind(() =>
        {
            DestroyWindow(windowBase);
        });
        if (isOpen)
            windowBase.transform.DOPlayForward();
        else
            windowBase.transform.DOPlayBackwards();
    }

    #endregion

    #region DestroyWindow 销毁窗口
    /// <summary>
    /// 销毁窗口
    /// </summary>
    /// <param name="obj"></param>
    private void DestroyWindow(UIWindowViewBase windowBase)
    {
        m_DicWindow.Remove(windowBase.ViewName);
        UnityEngine.Object.Destroy(windowBase.gameObject);
    }
    #endregion
}