﻿/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using EPiServer.Logging;
using Mediachase.Commerce.Orders;
using OxxCommerceStarterKit.Web.Models.ViewModels.Email;
using OxxCommerceStarterKit.Web.Services.Email.Models;

namespace OxxCommerceStarterKit.Web.Services.Email
{
    public class EmailService : IEmailService
    {
        private static readonly ILogger Log = LogManager.GetLogger();
        private readonly INotificationSettingsRepository _notificationSettingsRepository;
        private readonly IEmailDispatcher _emailDispatcher;

        public EmailService(INotificationSettingsRepository notificationSettingsRepository, IEmailDispatcher emailDispatcher)
        {
            _notificationSettingsRepository = notificationSettingsRepository;
            _emailDispatcher = emailDispatcher;
        }

        public bool SendResetPasswordEmail(string email, string subject, string body, string passwordHash, string resetUrl)
        {
            var mailSettings = _notificationSettingsRepository.GetNotificationSettings();
            if (mailSettings != null)
                return AttemptSendOf(CreateResetPasswordEmailMessage(email, subject, body, passwordHash, resetUrl, mailSettings));
            Log.Error("Unable to get notification settings");
            return false;
        }

        private static Models.ResetPassword CreateResetPasswordEmailMessage(string email, string subject, string body,
                                                                     string passwordHash, string resetUrl,
                                                                     NotificationSettings mailSettings)
        {
            var emailMessage = new Models.ResetPassword();
            emailMessage.From = mailSettings.From;
            emailMessage.To = email;
            emailMessage.Subject = subject;
            emailMessage.Header = mailSettings.MailHeader.ToString();
            emailMessage.Footer = mailSettings.MailFooter.ToString();
            emailMessage.Body = body;
            emailMessage.Token = passwordHash;
            emailMessage.ResetUrl = resetUrl;
            return emailMessage;
        }

        private bool AttemptSendOf(Models.ResetPassword emailMessage)
        {
            try
            {
                return Send(emailMessage);
            }
            catch (Exception ex)
            {
                Log.Error("Error occured when sending ResetPassword emailMessage.", ex);
                return false;
            }
        }

        private bool Send(Models.ResetPassword emailMessage)
        {
            var result = _emailDispatcher.SendEmail(emailMessage, Log);
            if (result.Success)
                return true;
            Log.Error(result.Exception.Message, result.Exception);
            return false;
        }

        public bool SendWelcomeEmail(string email, string subject, string body)
        {

            var mailSettings = _notificationSettingsRepository.GetNotificationSettings();
            if (mailSettings != null)
            {
                var emailMessage = new Welcome();
                emailMessage.From = mailSettings.From;
                emailMessage.To = email;
                emailMessage.Subject = subject;
                emailMessage.Header = mailSettings.MailHeader.ToString();
                emailMessage.Footer = mailSettings.MailFooter.ToString();
                emailMessage.Body = body;
                var result = _emailDispatcher.SendEmail(emailMessage, Log);
                if (result.Success)
                {
                    return true;
                }
                else
                {
                    Log.Error(result.Exception.Message, result.Exception);
                    return false;
                }
            }
            Log.Error("Unable to get notification settings");
            return false;
        }

        public bool SendOrderReceipt(PurchaseOrder order)
        {
            var mailSettings = _notificationSettingsRepository.GetNotificationSettings();
            if (mailSettings != null)
            {
                var emailMessage = new Receipt(order);
                emailMessage.From = mailSettings.From;
                emailMessage.Header = mailSettings.MailHeader.ToString();
                emailMessage.Footer = mailSettings.MailFooter.ToString();

                var result = _emailDispatcher.SendEmail(emailMessage);
                if (result.Success)
                {
                    return true;
                }
                else
                {
                    Log.Error(result.Exception.Message, result.Exception);
                    return false;
                }
            }
            Log.Error("Unable to get notification settings");
            return false;
        }

        public bool SendDeliveryReceipt(PurchaseOrder order, string language = null)
        {
            var mailSettings = _notificationSettingsRepository.GetNotificationSettings(language);
            if (mailSettings != null)
            {
                var emailMessage = new DeliveryReceipt(order);
                emailMessage.From = mailSettings.From;
                emailMessage.Header = mailSettings.MailHeader.ToString();
                emailMessage.Footer = mailSettings.MailFooter.ToString();

                var result = _emailDispatcher.SendEmail(emailMessage);
                if (result.Success)
                {
                    return true;
                }
                else
                {
                    Log.Error(result.Exception.Message, result.Exception);
                    return false;
                }
            }
            Log.Error("Unable to get notification settings");
            return false;
        }
    }

}
