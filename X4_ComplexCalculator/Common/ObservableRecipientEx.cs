using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace X4_ComplexCalculator.Common;


/// <summary>
/// <see cref="ObservableRecipient.Broadcast{T}(T, T, string?)"/> 時にトークンを自動付与するクラス
/// </summary>
public class ObservableRecipientEx : ObservableRecipient
{
    /// <summary>
    /// 派生クラス名
    /// </summary>
    private readonly string _className;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public ObservableRecipientEx(bool isActive = false) : base()
    {
        _className = GetType().Name;
        IsActive = isActive;
    }


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messenger"></param>
    public ObservableRecipientEx(IMessenger messenger, bool isActive = false) : base(messenger)
    {
        _className = GetType().Name;
        IsActive = isActive;
    }


    /// <inheritdoc/>
    protected override void Broadcast<T>(T oldValue, T newValue, string? propertyName)
    {
        if (IsActive)
        {
            Messenger.Send(new PropertyChangedMessage<T>(this, propertyName, oldValue, newValue), $"{_className}.{propertyName}");
        }
    }
}
