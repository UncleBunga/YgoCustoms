-- Slasher's Tow Truck
local s,id=GetID()
function s.initial_effect(c)
    c:EnableCounterPermit(0x199,LOCATION_SZONE)
    -- Activate: you can add 1 "Cannibal" monster from your Deck to your hand (only 1 activation per turn)
    local e1=Effect.CreateEffect(c)
    e1:SetDescription(aux.Stringid(id,0))
    e1:SetCategory(CATEGORY_TOHAND+CATEGORY_SEARCH)
    e1:SetType(EFFECT_TYPE_ACTIVATE)
    e1:SetCode(EVENT_FREE_CHAIN)
    e1:SetCountLimit(1,id,EFFECT_COUNT_CODE_OATH)
    e1:SetOperation(s.activate)
    c:RegisterEffect(e1)

    -- Each time a "Slasher" monster(s) is Normal or Special Summoned, place 1 Kill Counter on this card
    local e2=Effect.CreateEffect(c)
    e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
    e2:SetCode(EVENT_SUMMON_SUCCESS)
    e2:SetRange(LOCATION_SZONE)
    e2:SetCondition(s.ctcon)
    e2:SetOperation(s.ctop)
    c:RegisterEffect(e2)
    local e3=e2:Clone()
    e3:SetCode(EVENT_SPSUMMON_SUCCESS)
    c:RegisterEffect(e3)
end
s.counter_list={0x199}

function s.thfilter(c)
    return c:IsSetCard(0x1991) and c:IsMonster() and c:IsAbleToHand()
end

function s.activate(e,tp,eg,ep,ev,re,r,rp)
    local g=Duel.GetMatchingGroup(s.thfilter,tp,LOCATION_DECK,0,nil)
    if #g>0 and Duel.SelectYesNo(tp,aux.Stringid(id,1)) then
        Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_ATOHAND)
        local sg=g:Select(tp,1,1,nil)
        Duel.SendtoHand(sg,nil,REASON_EFFECT)
        Duel.ConfirmCards(1-tp,sg)
    end
end

function s.ctfilter(c)
    return c:IsFaceup() and c:IsSetCard(0x991)
end

function s.ctcon(e,tp,eg,ep,ev,re,r,rp)
    return eg:IsExists(s.ctfilter,1,nil)
end

function s.ctop(e,tp,eg,ep,ev,re,r,rp)
    e:GetHandler():AddCounter(0x199,1)
end
