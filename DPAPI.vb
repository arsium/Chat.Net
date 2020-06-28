Imports System
Imports System.Text
Imports System.Runtime.InteropServices
Imports System.ComponentModel
Imports Microsoft.VisualBasic

'From : https://freesilo.com/?p=452
Public Class DPAPI
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ' SAMPLE: Encryption and decryption using DPAPI functions.
    '
    ' To run this sample, create a new Visual Basic.NET project using the Console
    ' Application template and replace the contents of the Module1.vb file with
    ' the code below.
    '
    ' THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
    ' KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
    ' IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
    ' PURPOSE.
    '
    ' Copyright (C) 2003 Obviex(TM). All rights reserved.
    '




    ' Wrapper for DPAPI CryptProtectData function.
    <DllImport("crypt32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function CryptProtectData _
            (
                ByRef pPlainText As DATA_BLOB,
                ByVal szDescription As String,
                ByRef pEntropy As DATA_BLOB,
                ByVal pReserved As IntPtr,
                ByRef pPrompt As CRYPTPROTECT_PROMPTSTRUCT,
                ByVal dwFlags As Integer,
                ByRef pCipherText As DATA_BLOB
            ) As Boolean
    End Function

    ' Wrapper for DPAPI CryptUnprotectData function.
    <DllImport("crypt32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function CryptUnprotectData _
            (
                ByRef pCipherText As DATA_BLOB,
                ByRef pszDescription As String,
                ByRef pEntropy As DATA_BLOB,
                ByVal pReserved As IntPtr,
                ByRef pPrompt As CRYPTPROTECT_PROMPTSTRUCT,
                ByVal dwFlags As Integer,
                ByRef pPlainText As DATA_BLOB
            ) As Boolean
    End Function

    ' BLOB structure used to pass data to DPAPI functions.
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Friend Structure DATA_BLOB
        Public cbData As Integer
        Public pbData As IntPtr
    End Structure

    ' Prompt structure to be used for required parameters.
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Friend Structure CRYPTPROTECT_PROMPTSTRUCT
        Public cbSize As Integer
        Public dwPromptFlags As Integer
        Public hwndApp As IntPtr
        Public szPrompt As String
    End Structure

    ' DPAPI key initialization flags.
    Private Const CRYPTPROTECT_UI_FORBIDDEN As Integer = 1
    Private Const CRYPTPROTECT_LOCAL_MACHINE As Integer = 4

    ' <summary>
    ' Initializes empty prompt structure.
    ' </summary>
    ' <param name="ps">
    ' Prompt parameter (which we do not actually need).
    ' </param>
    Private Shared Sub InitPrompt _
            (
                ByRef ps As CRYPTPROTECT_PROMPTSTRUCT
            )
        ps.cbSize = Marshal.SizeOf(GetType(CRYPTPROTECT_PROMPTSTRUCT))
        ps.dwPromptFlags = 0
        ps.hwndApp = IntPtr.Zero
        ps.szPrompt = Nothing
    End Sub

    ' <summary>
    ' Initializes a BLOB structure from a byte array.
    ' </summary>
    ' <param name="data">
    ' Original data in a byte array format.
    ' </param>
    ' <param name="blob">
    ' Returned blob structure.
    ' </param>
    Private Shared Sub InitBLOB _
            (
                ByVal data As Byte(),
                ByRef blob As DATA_BLOB
            )
        ' Use empty array for null parameter.
        If data Is Nothing Then
            data = New Byte(0) {}
        End If

        ' Allocate memory for the BLOB data.
        blob.pbData = Marshal.AllocHGlobal(data.Length)

        ' Make sure that memory allocation was successful.
        If blob.pbData.Equals(IntPtr.Zero) Then
            Throw New Exception(
                            "Unable to allocate data buffer for BLOB structure.")
        End If

        ' Specify number of bytes in the BLOB.
        blob.cbData = data.Length
        Marshal.Copy(data, 0, blob.pbData, data.Length)
    End Sub

    ' Flag indicating the type of key. DPAPI terminology refers to
    ' key types as user store or machine store.
    Public Enum KeyType
        UserKey = 1
        MachineKey
    End Enum

    ' It is reasonable to set default key type to user key.
    Private Shared defaultKeyType As KeyType = KeyType.UserKey

    ' <summary>
    ' Calls DPAPI CryptProtectData function to encrypt a plaintext
    ' string value with a user-specific key. This function does not
    ' specify data description and additional entropy.
    ' </summary>
    ' <param name="plainText">
    ' Plaintext data to be encrypted.
    ' </param>
    ' <returns>
    ' Encrypted value in a base64-encoded format.
    ' </returns>
    Public Shared Function Encrypt _
            (
                ByVal plainText As String
            ) As String
        Return Encrypt(defaultKeyType, plainText, String.Empty, String.Empty)
    End Function

    ' <summary>
    ' Calls DPAPI CryptProtectData function to encrypt a plaintext
    ' string value. This function does not specify data description
    ' and additional entropy.
    ' </summary>
    ' <param name="keyType">
    ' Defines type of encryption key to use. When user key is
    ' specified, any application running under the same user account
    ' as the one making this call, will be able to decrypt data.
    ' Machine key will allow any application running on the same
    ' computer where data were encrypted to perform decryption.
    ' Note: If optional entropy is specifed, it will be required
    ' for decryption.
    ' </param>
    ' <param name="plainText">
    ' Plaintext data to be encrypted.
    ' </param>
    ' <returns>
    ' Encrypted value in a base64-encoded format.
    ' </returns>
    Public Shared Function Encrypt _
            (
                ByVal keyType As KeyType,
                ByVal plainText As String
            ) As String
        Return Encrypt(keyType, plainText, String.Empty, String.Empty)
    End Function

    Public Shared Function Encrypt _
            (
                ByVal keyType As KeyType,
                ByVal plainText As String,
                ByVal entropy As String
            ) As String
        Return Encrypt(keyType, plainText, entropy, String.Empty)
    End Function

    ' <summary>
    ' Calls DPAPI CryptProtectData function to encrypt a plaintext
    ' string value. This function does not specify data description.
    ' </summary>
    ' <param name="keyType">
    ' Defines type of encryption key to use. When user key is
    ' specified, any application running under the same user account
    ' as the one making this call, will be able to decrypt data.
    ' Machine key will allow any application running on the same
    ' computer where data were encrypted to perform decryption.
    ' Note: If optional entropy is specifed, it will be required
    ' for decryption.
    ' </param>
    ' <param name="plainText">
    ' Plaintext data to be encrypted.
    ' </param>
    ' <param name="entropy">
    ' Optional entropy which - if specified - will be required to
    ' perform decryption.
    ' </param>
    ' <returns>
    ' Encrypted value in a base64-encoded format.
    ' </returns>
    Public Shared Function Encrypt _
            (
                ByVal keyType As KeyType,
                ByVal plainText As String,
                ByVal entropy As String,
                ByVal description As String
            ) As String
        If plainText Is Nothing Then
            plainText = String.Empty
        End If
        If entropy Is Nothing Then
            entropy = String.Empty
        End If
        Return Convert.ToBase64String(
                    Encrypt(keyType,
                            Encoding.UTF8.GetBytes(plainText),
                            Encoding.UTF8.GetBytes(entropy),
                            description))
    End Function

    ' <summary>
    ' Calls DPAPI CryptProtectData function to encrypt an array of
    ' plaintext bytes.
    ' </summary>
    ' <param name="keyType">
    ' Defines type of encryption key to use. When user key is
    ' specified, any application running under the same user account
    ' as the one making this call, will be able to decrypt data.
    ' Machine key will allow any application running on the same
    ' computer where data were encrypted to perform decryption.
    ' Note: If optional entropy is specifed, it will be required
    ' for decryption.
    ' </param>
    ' <param name="plainTextBytes">
    ' Plaintext data to be encrypted.
    ' </param>
    ' <param name="entropyBytes">
    ' Optional entropy which - if specified - will be required to
    ' perform decryption.
    ' </param>
    ' <param name="description">
    ' Optional description of data to be encrypted. If this value is
    ' specified, it will be stored along with encrypted data and
    ' returned as a separate value during decryption.
    ' </param>
    ' <returns>
    ' Encrypted value.
    ' </returns>
    Public Shared Function Encrypt _
            (
                ByVal keyType As KeyType,
                ByVal plainTextBytes As Byte(),
                ByVal entropyBytes As Byte(),
                ByVal description As String
            ) As Byte()
        ' Make sure that parameters are valid.
        If plainTextBytes Is Nothing Then
            plainTextBytes = New Byte(0) {}
        End If

        If entropyBytes Is Nothing Then
            entropyBytes = New Byte(0) {}
        End If

        If description Is Nothing Then
            description = String.Empty
        End If

        ' Create BLOBs to hold data.
        Dim plainTextBlob As DATA_BLOB = New DATA_BLOB
        Dim cipherTextBlob As DATA_BLOB = New DATA_BLOB
        Dim entropyBlob As DATA_BLOB = New DATA_BLOB

        ' We only need prompt structure because it is a required
        ' parameter.
        Dim prompt As _
                        CRYPTPROTECT_PROMPTSTRUCT = New CRYPTPROTECT_PROMPTSTRUCT
        InitPrompt(prompt)

        Try
            ' Convert plaintext bytes into a BLOB structure.
            Try
                InitBLOB(plainTextBytes, plainTextBlob)
            Catch ex As Exception
                Throw New Exception("Cannot initialize plaintext BLOB.", ex)
            End Try

            ' Convert entropy bytes into a BLOB structure.
            Try
                InitBLOB(entropyBytes, entropyBlob)
            Catch ex As Exception
                Throw New Exception("Cannot initialize entropy BLOB.", ex)
            End Try

            ' Disable any types of UI.
            Dim flags As Integer = CRYPTPROTECT_UI_FORBIDDEN

            ' When using machine-specific key, set up machine flag.
            If keyType = KeyType.MachineKey Then
                flags = flags Or (CRYPTPROTECT_LOCAL_MACHINE)
            End If

            ' Call DPAPI to encrypt data.
            Dim success As Boolean = CryptProtectData(
                                                    plainTextBlob,
                                                    description,
                                                    entropyBlob,
                                                    IntPtr.Zero,
                                                    prompt,
                                                    flags,
                                                    cipherTextBlob)

            ' Check the result.
            If Not success Then
                ' If operation failed, retrieve last Win32 error.
                Dim errCode As Integer = Marshal.GetLastWin32Error()

                ' Win32Exception will contain error message corresponding
                ' to the Windows error code.
                Throw New Exception("CryptProtectData failed.",
                                        New Win32Exception(errCode))
            End If

            ' Allocate memory to hold ciphertext.
            Dim cipherTextBytes(cipherTextBlob.cbData - 1) As Byte

            ' Copy ciphertext from the BLOB to a byte array.
            Marshal.Copy(cipherTextBlob.pbData, cipherTextBytes, 0,
                                    cipherTextBlob.cbData)

            ' Return the result.
            Return cipherTextBytes
        Catch ex As Exception
            Throw New Exception("DPAPI was unable to encrypt data.", ex)
        Finally
            If Not (plainTextBlob.pbData.Equals(IntPtr.Zero)) Then
                Marshal.FreeHGlobal(plainTextBlob.pbData)
            End If

            If Not (cipherTextBlob.pbData.Equals(IntPtr.Zero)) Then
                Marshal.FreeHGlobal(cipherTextBlob.pbData)
            End If

            If Not (entropyBlob.pbData.Equals(IntPtr.Zero)) Then
                Marshal.FreeHGlobal(entropyBlob.pbData)
            End If
        End Try
    End Function

    ' <summary>
    ' Calls DPAPI CryptUnprotectData to decrypt ciphertext bytes.
    ' This function does not use additional entropy and does not
    ' return data description.
    ' </summary>
    ' <param name="cipherText">
    ' Encrypted data formatted as a base64-encoded string.
    ' </param>
    ' <returns>
    ' Decrypted data returned as a UTF-8 string.
    ' </returns>
    ' <remarks>
    ' When decrypting data, it is not necessary to specify which
    ' type of encryption key to use: user-specific or
    ' machine-specific; DPAPI will figure it out by looking at
    ' the signature of encrypted data.
    ' </remarks>
    Public Shared Function Decrypt _
            (
                ByVal cipherText As String
            ) As String
        Dim description As String
        Return Decrypt(cipherText, String.Empty, description)
    End Function

    ' <summary>
    ' Calls DPAPI CryptUnprotectData to decrypt ciphertext bytes.
    ' This function does not use additional entropy.
    ' </summary>
    ' <param name="cipherText">
    ' Encrypted data formatted as a base64-encoded string.
    ' </param>
    ' <param name="description">
    ' Returned description of data specified during encryption.
    ' </param>
    ' <returns>
    ' Decrypted data returned as a UTF-8 string.
    ' </returns>
    ' <remarks>
    ' When decrypting data, it is not necessary to specify which
    ' type of encryption key to use: user-specific or
    ' machine-specific; DPAPI will figure it out by looking at
    ' the signature of encrypted data.
    ' </remarks>
    Public Shared Function Decrypt _
            (
                ByVal cipherText As String,
                ByRef description As String
            ) As String
        Return Decrypt(cipherText, String.Empty, description)
    End Function

    ' <summary>
    ' Calls DPAPI CryptUnprotectData to decrypt ciphertext bytes.
    ' </summary>
    ' <param name="cipherText">
    ' Encrypted data formatted as a base64-encoded string.
    ' </param>
    ' <param name="entropy">
    ' Optional entropy, which is required if it was specified during
    ' encryption.
    ' </param>
    ' <param name="description">
    ' Returned description of data specified during encryption.
    ' </param>
    ' <returns>
    ' Decrypted data returned as a UTF-8 string.
    ' </returns>
    ' <remarks>
    ' When decrypting data, it is not necessary to specify which
    ' type of encryption key to use: user-specific or
    ' machine-specific; DPAPI will figure it out by looking at
    ' the signature of encrypted data.
    ' </remarks>
    Public Shared Function Decrypt _
            (
                ByVal cipherText As String,
                ByVal entropy As String,
                ByRef description As String
            ) As String
        ' Make sure that parameters are valid.
        If entropy Is Nothing Then
            entropy = String.Empty
        End If

        Return Encoding.UTF8.GetString(
                    Decrypt(Convert.FromBase64String(cipherText),
                            Encoding.UTF8.GetBytes(entropy), description))
    End Function

    ' <summary>
    ' Calls DPAPI CryptUnprotectData to decrypt ciphertext bytes.
    ' </summary>
    ' <param name="cipherTextBytes">
    ' Encrypted data.
    ' </param>
    ' <param name="entropyBytes">
    ' Optional entropy, which is required if it was specified during
    ' encryption.
    ' </param>
    ' <param name="description">
    ' Returned description of data specified during encryption.
    ' </param>
    ' <returns>
    ' Decrypted data bytes.
    ' </returns>
    ' <remarks>
    ' When decrypting data, it is not necessary to specify which
    ' type of encryption key to use: user-specific or
    ' machine-specific; DPAPI will figure it out by looking at
    ' the signature of encrypted data.
    ' </remarks>
    Public Shared Function Decrypt _
            (
                ByVal cipherTextBytes As Byte(),
                ByVal entropyBytes As Byte(),
                ByRef description As String
            ) As Byte()

        ' Create BLOBs to hold data.
        Dim plainTextBlob As DATA_BLOB = New DATA_BLOB
        Dim cipherTextBlob As DATA_BLOB = New DATA_BLOB
        Dim entropyBlob As DATA_BLOB = New DATA_BLOB

        ' We only need prompt structure because it is a required
        ' parameter.
        Dim prompt As _
                        CRYPTPROTECT_PROMPTSTRUCT = New CRYPTPROTECT_PROMPTSTRUCT
        InitPrompt(prompt)

        ' Initialize description string.
        description = String.Empty

        Try
            ' Convert ciphertext bytes into a BLOB structure.
            Try
                InitBLOB(cipherTextBytes, cipherTextBlob)
            Catch ex As Exception
                Throw New Exception("Cannot initialize ciphertext BLOB.", ex)
            End Try

            ' Convert entropy bytes into a BLOB structure.
            Try
                InitBLOB(entropyBytes, entropyBlob)
            Catch ex As Exception
                Throw New Exception("Cannot initialize entropy BLOB.", ex)
            End Try

            ' Disable any types of UI. CryptUnprotectData does not
            ' mention CRYPTPROTECT_LOCAL_MACHINE flag in the list of
            ' supported flags so we will not set it up.
            Dim flags As Integer = CRYPTPROTECT_UI_FORBIDDEN

            ' Call DPAPI to decrypt data.
            Dim success As Boolean = CryptUnprotectData(
                                                    cipherTextBlob,
                                                    description,
                                                    entropyBlob,
                                                    IntPtr.Zero,
                                                    prompt,
                                                    flags,
                                                    plainTextBlob)

            ' Check the result.
            If Not success Then
                ' If operation failed, retrieve last Win32 error.
                Dim errCode As Integer = Marshal.GetLastWin32Error()

                ' Win32Exception will contain error message corresponding
                ' to the Windows error code.
                Throw New Exception("CryptUnprotectData failed.",
                                    New Win32Exception(errCode))
            End If

            ' Allocate memory to hold plaintext.
            Dim plainTextBytes(plainTextBlob.cbData - 1) As Byte

            ' Copy ciphertext from the BLOB to a byte array.
            Marshal.Copy(plainTextBlob.pbData, plainTextBytes, 0,
                                    plainTextBlob.cbData)

            ' Return the result.
            Return plainTextBytes
        Catch ex As Exception
            Throw New Exception("DPAPI was unable to decrypt data.", ex)
            ' Free all memory allocated for BLOBs.
        Finally
            If Not (plainTextBlob.pbData.Equals(IntPtr.Zero)) Then
                Marshal.FreeHGlobal(plainTextBlob.pbData)
            End If

            If Not (cipherTextBlob.pbData.Equals(IntPtr.Zero)) Then
                Marshal.FreeHGlobal(cipherTextBlob.pbData)
            End If

            If Not (entropyBlob.pbData.Equals(IntPtr.Zero)) Then
                Marshal.FreeHGlobal(entropyBlob.pbData)
            End If
        End Try
    End Function

End Class
