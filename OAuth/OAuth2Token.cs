namespace Common.OAuth;

public record OAuth2Token(string Type, string Value, long Expire);