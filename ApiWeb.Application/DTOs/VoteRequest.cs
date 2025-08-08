namespace ApiWeb.Application.DTOs;

using System.ComponentModel.DataAnnotations;

public class VoteRequest
{
    [Range(-1, 1)]
    public int Value { get; set; }
}