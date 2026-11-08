namespace sloppr.Services;

public class RecipeTemplateNotFoundException(string message) : Exception(message);

public class RecipeTemplateConflictException(string message) : Exception(message);

public class RecipeTemplateValidationException(string message) : Exception(message);
