using UnityEngine;

public class ResultSE : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _resultSE;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// アニメーションから呼び出され、結果音声を再生する
    /// </summary>
    public void PlayResultSE()
    {
        _audioSource.PlayOneShot(_resultSE);
    }
}
