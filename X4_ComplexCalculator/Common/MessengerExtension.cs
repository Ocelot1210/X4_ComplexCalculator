using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Linq.Expressions;

namespace X4_ComplexCalculator.Common;


/// <summary>
/// <see cref="IMessenger"/> の拡張メソッドを定義するクラス
/// </summary>
internal static class MessengerExtension
{
    /// <summary>
    /// <see cref="PropertyChangedMessage{T}">の登録補助</see>
    /// </summary>
    /// <typeparam name="TRecipient"></typeparam>
    /// <typeparam name="TObservable"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="messenger"></param>
    /// <param name="recipient"></param>
    /// <param name="expr"></param>
    /// <param name="handler"></param>
    public static void RegisterPropertyChangedMessage<TRecipient, TObservable, TProperty>(
        this IMessenger messenger,
             TRecipient recipient,
             Expression<Func<TObservable, TProperty>> expr,
             MessageHandler<TRecipient, PropertyChangedMessage<TProperty>> handler
    ) where TRecipient : ObservableRecipient
    {
        messenger.Register(recipient, MakeToken(expr), handler);
    }


    /// <summary>
    /// クラス名とメンバ名からトークン文字列を作成
    /// </summary>
    /// <typeparam name="TObservable"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="expr"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static string MakeToken<TObservable, TProperty>(Expression<Func<TObservable, TProperty>> expr)
    {
        var className = expr.Parameters[0].Type.Name;
        var memberName = (expr.Body as MemberExpression)?.Member.Name;
        if (string.IsNullOrEmpty(memberName))
        {
            throw new ArgumentException("", nameof(expr));
        }

        return $"{className}.{memberName}";
    }


    /// <summary>
    /// <see cref="RequestMessage{T}"/> の登録補助
    /// </summary>
    /// <typeparam name="TRecipient"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="messenger"></param>
    /// <param name="recipient"></param>
    /// <param name="getter"></param>
    public static void RegisterRequestMessage<TRecipient, TResult>(
        this IMessenger messenger,
        TRecipient recipient,
        Func<TRecipient, TResult> getter
    ) where TRecipient : ObservableRecipient
    {
        messenger.Register<TRecipient, RequestMessage<TResult>>(recipient, (r, m) => m.Reply(getter(recipient)));
    }
}