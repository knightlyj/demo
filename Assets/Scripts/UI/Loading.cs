using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public delegate void OnLoadingDone();

public class Loading : MonoBehaviour, IPointerClickHandler {
    public event OnLoadingDone onLoadingDone = null;

    [SerializeField]
    Text txtTips = null;
	
    enum LoadingState
    {
        Nothing,
        Loading,
        Done,
        WaitForClick,
    }

    int count = 0;
	// Update is called once per frame
	void Update () {
        if (state == LoadingState.Loading)
        {
            count++;
            if (count > 5)  //不要每一帧都处理  
            {
                count = 0;
                float progress = asyncOperation.progress;
                if (progress < 0.80f)
                {
                    txtTips.text = string.Format("加载中: {0:0%}", progress);
                }
                else
                {
                    state = LoadingState.Done;
                    if (onLoadingDone != null)
                        onLoadingDone();
                }
            }
        }
	}

    LoadingState state = LoadingState.Nothing;
    AsyncOperation asyncOperation = null;
    public void StartLoading(string name)
    {
        if (state == LoadingState.Nothing)
        {
            asyncOperation = SceneManager.LoadSceneAsync(name);
            asyncOperation.allowSceneActivation = false;
            state = LoadingState.Loading;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if(state == LoadingState.WaitForClick)
        {
            if (asyncOperation != null)
            {
                asyncOperation.allowSceneActivation = true  ;
            }
        }
    }

    public void WaitForClick()
    {
        if(state == LoadingState.Done)
        {
            state = LoadingState.WaitForClick;
            txtTips.text = "加载完成,点击继续";
        }
    }
}
