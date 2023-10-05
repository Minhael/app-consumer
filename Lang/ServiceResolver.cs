namespace Common.Lang;

//  https://stackoverflow.com/questions/39174989/how-to-register-multiple-implementations-of-the-same-interface-in-asp-net-core
public delegate T ServiceResolver<T>(string key);