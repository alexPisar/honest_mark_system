using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace WebSystems
{
    public enum DocumentFormatsEnum
    {
        [EnumMember(Value = "NONE")]
        None = 0,

        [EnumMember(Value = "MANUAL")]
        Manual,

        [EnumMember(Value = "XML")]
        Xml,

        [EnumMember(Value = "CSV")]
        Csv
    }

    public enum ProductGroupsEnum
    {
        [EnumMember(Value = "none")]
        None = 0,

        /// <summary>
        ///Предметы одежды, бельё постельное, столовое, туалетное и кухонное
        /// </summary>
        [EnumMember(Value = "lp")]
        Lp,

        /// <summary>
        ///Обувные товары
        /// </summary>
        [EnumMember(Value = "shoes")]
        Shoes,

        /// <summary>
        ///Табачная продукция
        /// </summary>
        [EnumMember(Value = "tobacco")]
        Tobacco,

        /// <summary>
        ///Духи и туалетная вода
        /// </summary>
        [EnumMember(Value = "perfumery")]
        Perfumery,

        /// <summary>
        ///Шины и покрышки пневматические резиновые новые
        /// </summary>
        [EnumMember(Value = "tires")]
        Tires,

        /// <summary>
        ///Фотокамеры (кроме кинокамер), фотовспышки и лампы-вспышки
        /// </summary>
        [EnumMember(Value = "electronics")]
        Electronics,

        /// <summary>
        ///Молочная продукция
        /// </summary>
        [EnumMember(Value = "milk")]
        Milk = 8,

        /// <summary>
        ///Велосипеды и велосипедные рамы
        /// </summary>
        [EnumMember(Value = "bicycle")]
        Bicycle,

        /// <summary>
        ///Кресла-коляски
        /// </summary>
        [EnumMember(Value = "wheelchairs")]
        Wheelchairs,

        /// <summary>
        ///Альтернативная табачная продукция
        /// </summary>
        [EnumMember(Value = "otp")]
        Otp = 12,

        /// <summary>
        ///Упакованная вода
        /// </summary>
        [EnumMember(Value = "water")]
        Water,

        /// <summary>
        ///Товары из натурального меха
        /// </summary>
        [EnumMember(Value = "furs")]
        Furs,

        /// <summary>
        ///Пиво, напитки, изготавливаемые на основе пива, слабоалкогольные напитки
        /// </summary>
        [EnumMember(Value = "beer")]
        Beer,

        /// <summary>
        ///Никотиносодержащая продукция
        /// </summary>
        [EnumMember(Value = "ncp")]
        Ncp,

        /// <summary>
        ///Биологические активные добавки к пище
        /// </summary>
        [EnumMember(Value = "bio")]
        Bio
    }

    public enum DocumentProcessStatusesEnum
    {
        [EnumMember(Value = "none")]
        None = 0,

        /// <summary>
        /// Документ обрабатывается
        /// </summary>
        [EnumMember(Value = "IN_PROGRESS")]
        InProgress,

        /// <summary>
        /// Документ обработан с ошибками
        /// </summary>
        [EnumMember(Value = "CHECKED_NOT_OK")]
        CheckedNotOk,

        /// <summary>
        /// Документ обработан с ошибками
        /// </summary>
        [EnumMember(Value = "PARSE_ERROR")]
        ParseError,

        /// <summary>
        /// Техническая ошибка
        /// </summary>
        [EnumMember(Value = "PROCESSING_ERROR")]
        ProcessingError,

        /// <summary>
        /// Аннулирован
        /// </summary>
        [EnumMember(Value = "CANCELLED")]
        Cancelled,

        /// <summary>
        /// Ожидает регистрации участника в ГИС МТ
        /// </summary>
        [EnumMember(Value = "WAIT_PARTICIPANT_REGISTRATION")]
        WaitRarticipantRegistration,

        /// <summary>
        /// Ожидает продолжения обработки документа
        /// </summary>
        [EnumMember(Value = "WAIT_FOR_CONTINUATION")]
        WaitForContinuation,

        /// <summary>
        /// Ожидает приемку
        /// </summary>
        [EnumMember(Value = "WAIT_ACCEPTANCE")]
        WaitAcceptance,

        /// <summary>
        /// Документ успешно обработан
        /// </summary>
        [EnumMember(Value = "CHECKED_OK")]
        CheckedOk,

        /// <summary>
        /// Принят
        /// </summary>
        [EnumMember(Value = "ACCEPTED")]
        Accepted
    }

    public enum DocumentInOutType
    {
        None,
        Inbox,
        Outbox
    }

    public enum EdoLiteDocumentType
    {
        None,

        /// <summary>
        /// Уведомление об уточнении
        /// </summary>
        DpUvutoch = 110,

        /// <summary>
        /// Квитанция подтверждения даты получения
        /// </summary>
        DpPdpol,

        /// <summary>
        /// Квитанция подтверждения даты отправки
        /// </summary>
        DpPdotpr,

        /// <summary>
        /// Квитанция извещение о получении файла продавцом
        /// </summary>
        DpIzvpolSeller,

        /// <summary>
        /// Квитанция предложения об аннулировании документа
        /// </summary>
        DpPrannul,

        /// <summary>
        /// Квитанция извещение о получении файла покупателем
        /// </summary>
        DpIzvpolBuyer,

        /// <summary>
        /// УКД с функцией ДИС (корректировка накладной)
        /// </summary>
        DpUkdDis = 200,

        /// <summary>
        /// УКД с функцией ДИС информация покупателя (корректировка накладной)
        /// </summary>
        DpUkdDisInfoBuyer,

        /// <summary>
        /// УКД с функцией КСЧФ (корректировка счет-фактуры)
        /// </summary>
        DpUkdInvoice,

        /// <summary>
        /// УКД с функцией КСЧФДИС (корректировка счет-фактуры+накладная)
        /// </summary>
        DpUkdInvoiceDis = 204,

        /// <summary>
        /// УКД с функцией КСЧФДИС информация покупателя(корректировка счет-фактуры+накладная)
        /// </summary>
        DpUkdInvoiceDisInfoBuyer,

        /// <summary>
        /// УПД с функцией ДОП (Накладная)
        /// </summary>
        DpUpdDop = 500,

        /// <summary>
        /// УПД с функцией ДОП информация покупателя (Накладная)
        /// </summary>
        DpUpdDopInfoBuyer,

        /// <summary>
        /// УПД с функцией СЧФДОП (Счёт-фактура+Накладная)
        /// </summary>
        DpUpdInvoiceDop = 504,

        /// <summary>
        /// УПД с функцией СЧФДОП информация покупателя (Счёт-фактура+Накладная)
        /// </summary>
        DpUpdInvoiceDopInfoBuyer,

        /// <summary>
        /// УПД(и) с функцией ДОП (Накладная исправленная)
        /// </summary>
        DpUpdiDop = 800,

        /// <summary>
        /// УПД(и) с функцией ДОП информация покупателя (Накладная исправленная)
        /// </summary>
        DpUpdiDopInfoBuyer,

        /// <summary>
        /// УПД(и) с функцией СЧФДОП (Счёт-фактура исправленный + Накладная)
        /// </summary>
        DpUpdiInvoiceDop = 804,

        /// <summary>
        /// УПД(и) с функцией СЧФДОП информация покупателя (Счёт-фактура исправленный + Накладная)
        /// </summary>
        DpUpdiInvoiceDopInfoBuyer,
    }

    public enum DocEdoStatus
    {
        /// <summary>
        /// Отклонён
        /// </summary>
        Rejected = -1,

        /// <summary>
        /// Новый
        /// </summary>
        New,

        /// <summary>
        /// Отправлен
        /// </summary>
        Sent,

        /// <summary>
        /// Обработан
        /// </summary>
        Processed,

        /// <summary>
        /// Отказано в аннулировании
        /// </summary>
        RejectRevoke = 4,

        /// <summary>
        /// Ошибка обработки
        /// </summary>
        ProcessingError = 8,

        /// <summary>
        /// Не требуется подпись
        /// </summary>
        NoSignatureRequired = 10,

        /// <summary>
        /// Требуется аннулирование
        /// </summary>
        RevokeRequired,

        /// <summary>
        /// Запрошено аннулирование
        /// </summary>
        RevokeRequested,

        /// <summary>
        /// Аннулирован
        /// </summary>
        Revoked
    }

    public enum EdoLiteDocumentStatus
    {
        /// <summary>
        /// Черновик
        /// </summary>
        Draft,

        /// <summary>
        /// Отправлен
        /// </summary>
        Sent,

        /// <summary>
        /// Доставлен (подпись не требуется)
        /// </summary>
        Delivered,

        /// <summary>
        /// Доставлен, ожидается подпись
        /// </summary>
        DeliveredAwaitingSignature,

        /// <summary>
        /// Подписан
        /// </summary>
        Signed,

        /// <summary>
        /// Отклонён
        /// </summary>
        Rejected,

        /// <summary>
        /// Уточнён
        /// </summary>
        Clarified = 7,

        /// <summary>
        /// Ожидается уточнение
        /// </summary>
        ClarificationPending,

        /// <summary>
        /// Ошибка в подписи
        /// </summary>
        SignatureError,

        /// <summary>
        /// Ошибка доставки
        /// </summary>
        DeliveryError,

        /// <summary>
        /// Ожидается отправка
        /// </summary>
        AwaitingDispatch,

        /// <summary>
        /// Просмотрен (подпись не требуется)
        /// </summary>
        Viewed,

        /// <summary>
        /// Просмотрен (ожидается подпись)
        /// </summary>
        ViewedAwaitingSignature,

        /// <summary>
        /// Требуется уточнение (запрос на уточнение просмотрен)
        /// </summary>
        ClarificationRequired,

        /// <summary>
        /// Отклонен (запрос просмотрен)
        /// </summary>
        RejectedReviewed,

        /// <summary>
        /// Ожидается аннулирование
        /// </summary>
        СancellationPending,

        /// <summary>
        /// Подписан и отправлен
        /// </summary>
        SignedAndSend = 61
    }

    public enum EdoLiteProcessResultStatus
    {
        /// <summary>
        /// Документ обработан  успешно
        /// </summary>
        SUCCESS,

        /// <summary>
        /// Документ обработан, но с ошибками
        /// </summary>
        FAILED,

        /// <summary>
        /// Документ в процессе обработки
        /// </summary>
        IN_PROGRESS
    }

    public enum ReasonOfWithdrawalFromTurnover
    {
        /// <summary>
        /// Розничная реализация
        /// </summary>
        [EnumMember(Value = "RETAIL")]
        [Description("Розничная реализация")]
        Retail = 1,

        /// <summary>
        /// Экспорт в страны ЕАЭС
        /// </summary>
        [EnumMember(Value = "EEC_EXPORT")]
        [Description("Экспорт в страны ЕАЭС")]
        EecExport,

        /// <summary>
        /// Экспорт за пределы стран ЕАЭС
        /// </summary>
        [EnumMember(Value = "BEYOND_EEC_EXPORT")]
        [Description("Экспорт за пределы стран ЕАЭС")]
        BeyondEecExport,

        /// <summary>
        /// Возврат физическому лицу
        /// </summary>
        [EnumMember(Value = "RETURN")]
        [Description("Возврат физическому лицу")]
        Return,

        /// <summary>
        /// Продажа по образцам, дистанционный способ продажи
        /// </summary>
        [EnumMember(Value = "REMOTE_SALE")]
        [Description("Продажа по образцам, дистанционный способ продажи")]
        RemoteSale,

        /// <summary>
        /// Утрата или повреждение
        /// </summary>
        [EnumMember(Value = "DAMAGE_LOSS")]
        [Description("Утрата или повреждение")]
        DamageLoss,

        /// <summary>
        /// Утилизация или уничтожение
        /// </summary>
        [EnumMember(Value = "DESTRUCTION")]
        [Description("Утилизация или уничтожение")]
        Destruction,

        /// <summary>
        /// Конфискация
        /// </summary>
        [EnumMember(Value = "CONFISCATION")]
        [Description("Конфискация")]
        Confiscation,

        /// <summary>
        /// Ликвидация предприятия
        /// </summary>
        [EnumMember(Value = "LIQUIDATION")]
        [Description("Ликвидация предприятия")]
        Liquidation,

        /// <summary>
        /// Использование для собственных нужд предприятия
        /// </summary>
        [EnumMember(Value = "ENTERPRISE_USE")]
        [Description("Использование для собственных нужд предприятия")]
        EnterpriseUse,

        /// <summary>
        /// Продажа по сделке, составляющей гос. тайну
        /// </summary>
        [EnumMember(Value = "STATE_SECRET")]
        [Description("Продажа по сделке, составляющей гос. тайну")]
        StateSecret,

        /// <summary>
        /// Другое
        /// </summary>
        [EnumMember(Value = "OTHER")]
        [Description("Другое")]
        Other
    }
}
