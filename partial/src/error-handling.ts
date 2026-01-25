import { logger } from "@cortexerr/core";

type HandledResult<T> =
  | { success: true; data: T }
  | { success: false; error: ErrorInfo };

type ErrorCode =
  // | "NETWORK_ERROR" // fetch failed, timeout, no connection
  // | "HTTP_ERROR" // got response but 4xx/5xx status
  // | "VALIDATION_ERROR" // input didn't pass validation
  | "NOT_FOUND" // resource doesn't exist
  // | "UNAUTHORIZED" // auth issue
  | "ALREADY_EXISTS"
  | "UNEXPECTED_ERROR" // caught exception, bugs
  | "INVALID_INPUT" // data does not match expected input
  | "TIMEOUT" // operation took too long
  | "RATE_LIMITED"; // too many requests

export type ErrorInfo = {
  code: ErrorCode;
  message: string;
  context?: Record<string, unknown>;
};
export function successResponse(): { success: true; data: undefined };
export function successResponse<T>(data: T): { success: true; data: T };
export function successResponse<T = undefined>(data?: T) {
  return {
    success: true,
    data: data as T,
  };
}

export function errorResponse(
  code: ErrorCode,
  message: string,
  context?: Record<string, unknown>,
) {
  logger.error(`[${code}] - ${message}`);
  const response: HandledResult<undefined> = {
    success: false,
    error: {
      code: code,
      message: message,
    },
  };
  if (context) {
    response.error.context = context;
  }
  return response;
}

export function handleError<FuncT extends (...args: unknown[]) => unknown>(
  func: FuncT,
  ...args: Parameters<FuncT>
): ReturnType<FuncT> | HandledResult<ReturnType<FuncT>> {
  try {
    const result = func(...args) as ReturnType<FuncT>;
    return { success: true, data: result };
  } catch (error) {
    // basic concept decide real implementation later
    logger.error(error);
    return {
      success: false,
      error: {
        code: "UNEXPECTED_ERROR",
        message: error instanceof Error ? error.message : "Unknown error",
        context: { stack: error instanceof Error ? error.stack : undefined },
      },
    };
  }
}

export async function handleErrorAsync<
  FuncT extends (...args: unknown[]) => Promise<unknown>,
>(
  func: FuncT,
  ...args: Parameters<FuncT>
): Promise<HandledResult<Awaited<ReturnType<FuncT>>>> {
  try {
    const result = (await func(...args)) as Awaited<ReturnType<FuncT>>;
    return { success: true, data: result };
  } catch (error) {
    // basic concept decide real implementation later
    logger.error(error);
    return {
      success: false,
      error: {
        code: "UNEXPECTED_ERROR",
        message: error instanceof Error ? error.message : "Unknown error",
        context: { stack: error instanceof Error ? error.stack : undefined },
      },
    };
  }
}
