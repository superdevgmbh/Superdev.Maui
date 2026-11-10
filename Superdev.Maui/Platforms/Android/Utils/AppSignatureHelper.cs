using System.Text;
using Android.Content;
using Android.Content.PM;
using Android.Util;
using Java.Security;
using Java.Util;
using Base64 = Android.Util.Base64;
using Signature = Android.Content.PM.Signature;

namespace Superdev.Maui.Platforms.Android.Utils
{
    /// <summary>
    ///     AppSignatureHelper calculates the current app's hash key.
    ///
    ///     Original Source:
    ///     https://github.com/googlearchive/android-credentials/blob/master/sms-verification/android/app/src/main/java/com/google/samples/smartlock/sms_verify/AppSignatureHelper.java
    ///
    ///     C# Translations:
    ///     https://github.com/search?l=C%23&amp;q=AppHashKeyHelper&amp;type=Code
    /// </summary>
    public static class AppSignatureHelper
    {
        private const string HashType = "SHA-256";
        private const int NumberOfHashedBytes = 9;
        private const int NumberOfBase64Chars = 11;

        /// <summary>
        ///     Retrieve the app signed package signature
        ///     known as 'signed keystore file hex string'.
        /// </summary>
        /// <remarks>
        ///     Android 28 replaced <see cref="PackageInfo.Signatures"/> with <see cref="PackageInfo.SigningInfo"/>. Both
        ///     paths take the first certificate, which is what <c>Signatures</c> returns on every version: the only
        ///     signer, the first of several, or the original one of a rotated key.
        /// </remarks>
        private static string? GetPackageSignature(Context context)
        {
            if (context.PackageManager is not PackageManager packageManager || context.PackageName is not string packageName)
            {
                return null;
            }

            Signature[]? signatures;
            if (OperatingSystem.IsAndroidVersionAtLeast(28))
            {
                var signingInfo = packageManager.GetPackageInfo(packageName, PackageInfoFlags.SigningCertificates)?.SigningInfo;
                signatures = signingInfo?.HasMultipleSigners == true
                    ? signingInfo.GetApkContentsSigners()
                    : signingInfo?.GetSigningCertificateHistory();
            }
            else
            {
                signatures = packageManager.GetPackageInfo(packageName, PackageInfoFlags.Signatures)?.Signatures?.ToArray();
            }

            return signatures?.FirstOrDefault()?.ToCharsString();
        }

        /// <summary>
        ///     Gets the current app's hash key.
        /// </summary>
        /// <returns>The app hash key, or <c>null</c> if the package's signature cannot be read.</returns>
        /// <param name="context">Android app Context.</param>
        public static string? GetAppHashKey(Context context)
        {
            if (GetPackageSignature(context) is not string keystoreHexSignature)
            {
                return null;
            }

            var appInfo = context.PackageName + " " + keystoreHexSignature;
            try
            {
                var messageDigest = MessageDigest.GetInstance(HashType);
                messageDigest.Update(Encoding.UTF8.GetBytes(appInfo));
                var hashSignature = messageDigest.Digest();

                hashSignature = Arrays.CopyOfRange(hashSignature, 0, NumberOfHashedBytes);
                var base64Hash = Base64.EncodeToString(hashSignature, Base64Flags.NoPadding | Base64Flags.NoWrap);
                base64Hash = base64Hash?.Substring(0, NumberOfBase64Chars);

                return base64Hash;
            }
            catch (NoSuchAlgorithmException)
            {
                return null;
            }
        }
    }
}
